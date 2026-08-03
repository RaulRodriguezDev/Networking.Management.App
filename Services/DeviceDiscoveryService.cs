using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;

namespace Network.Management.App.Services
{
    public class DeviceDiscoveryService : IDeviceDiscoveryService
    {
        // Bound the fan-out so a scan cannot exhaust sockets, and cap how many addresses a
        // single scan may cover to protect against an accidentally huge configured range.
        private const int MaxConcurrentProbes = 64;
        private const int ProbeTimeoutMs = 1000;
        private const int MaxScanSize = 65536;

        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;
        private readonly INetworkSettingService _networkSettingService;
        private readonly IReachabilityService _reachability;
        private readonly IMacAddressResolver _macResolver;

        public DeviceDiscoveryService(
            AppDbContext context,
            IAppLogger logger,
            INetworkSettingService networkSettingService,
            IReachabilityService reachability,
            IMacAddressResolver macResolver)
        {
            _context = context;
            _logger = logger;
            _networkSettingService = networkSettingService;
            _reachability = reachability;
            _macResolver = macResolver;
        }

        public async Task<DiscoveryResultDto> DiscoverAsync(int requestingUserId)
        {
            var range = await _networkSettingService.GetAsync();

            if (!IpAddressHelper.TryParse(range.RangeStart, out var start)
                || !IpAddressHelper.TryParse(range.RangeEnd, out var end)
                || start > end)
                throw new InvalidOperationException(
                    "The configured network range is invalid. Update it before scanning the network.");

            var scanSize = (long)end - start + 1;
            if (scanSize > MaxScanSize)
                throw new InvalidOperationException(
                    $"The configured network range covers {scanSize} addresses, which exceeds the {MaxScanSize} scan limit. Narrow the range and try again.");

            var registered = await _context.Devices.ToListAsync();

            var registeredByIp = registered
                .GroupBy(d => d.IpAddress, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            using var throttler = new SemaphoreSlim(MaxConcurrentProbes);
            var tasks = new List<Task<DiscoveredDeviceDto?>>();

            for (var value = start; value <= end; value++)
                tasks.Add(ProbeAsync(IpAddressHelper.ToIp(value), throttler));

            var probed = await Task.WhenAll(tasks);

            var discovered = probed
                .Where(r => r is not null)
                .Select(r => r!)
                .OrderBy(r => IpAddressHelper.TryParse(r.IpAddress, out var v) ? v : uint.MaxValue)
                .ToList();

            var macChanged = false;
            foreach (var host in discovered)
            {
                if (!registeredByIp.TryGetValue(host.IpAddress, out var reg)) continue;

                host.IsRegistered = true;
                host.DeviceId = reg.DeviceId;
                host.DeviceName = reg.DeviceName;

                // Keep the registered device's stored MAC in sync with what the scan resolved.
                if (host.MacAddress is not null
                    && !string.Equals(reg.MacAddress, host.MacAddress, StringComparison.OrdinalIgnoreCase))
                {
                    reg.MacAddress = host.MacAddress;
                    macChanged = true;
                }

                // Fall back to the stored MAC when live ARP resolution came up empty.
                host.MacAddress ??= reg.MacAddress;
            }

            if (macChanged)
                await _context.SaveChangesAsync();

            var discoveredIps = new HashSet<string>(
                discovered.Select(d => d.IpAddress), StringComparer.OrdinalIgnoreCase);

            var missing = registered
                .Where(d => !discoveredIps.Contains(d.IpAddress))
                .Select(d => new MissingDeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    MacAddress = d.MacAddress,
                    Status = d.Status
                })
                .ToList();

            await _logger.PersistAsync(
                LogActionType.DeviceDiscovered,
                $"Network scan of {range.RangeStart} - {range.RangeEnd} found {discovered.Count} live host(s); {missing.Count} registered device(s) did not respond.",
                requestingUserId);

            return new DiscoveryResultDto
            {
                RangeStart = range.RangeStart,
                RangeEnd = range.RangeEnd,
                ScannedCount = (int)scanSize,
                Discovered = discovered,
                Missing = missing
            };
        }

        private async Task<DiscoveredDeviceDto?> ProbeAsync(string ipAddress, SemaphoreSlim throttler)
        {
            await throttler.WaitAsync();
            try
            {
                var result = await _reachability.PingAsync(ipAddress, ProbeTimeoutMs);
                if (!result.Reachable) return null;

                return new DiscoveredDeviceDto
                {
                    IpAddress = ipAddress,
                    LatencyMs = result.LatencyMs,
                    MacAddress = await _macResolver.ResolveAsync(ipAddress)
                };
            }
            finally
            {
                throttler.Release();
            }
        }
    }
}
