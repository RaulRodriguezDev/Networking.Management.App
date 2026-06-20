using Microsoft.EntityFrameworkCore;
using Network.Management.App.Repository;

namespace Network.Management.App.Services
{
    /// <summary>
    /// Periodically pings every registered device to keep their Online/Offline status in sync
    /// with the real network. Status changes are detected and logged automatically.
    /// </summary>
    public class DeviceMonitorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IReachabilityService _reachability;
        private readonly ILogger<DeviceMonitorService> _logger;
        private readonly TimeSpan _interval;

        public DeviceMonitorService(
            IServiceScopeFactory scopeFactory,
            IReachabilityService reachability,
            IConfiguration configuration,
            ILogger<DeviceMonitorService> logger)
        {
            _scopeFactory = scopeFactory;
            _reachability = reachability;
            _logger = logger;

            var minutes = configuration.GetValue("Monitoring:IntervalMinutes", 2);
            _interval = TimeSpan.FromMinutes(minutes <= 0 ? 2 : minutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Small initial delay so the app finishes starting before the first sweep.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollAllAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DeviceMonitor] Polling sweep failed.");
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task PollAllAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var operations = scope.ServiceProvider.GetRequiredService<IDeviceOperationsService>();

            var devices = await context.Devices.ToListAsync(stoppingToken);
            if (devices.Count == 0) return;

            // Attribute automated status changes to the reserved 'system' account so they
            // appear in the Activity Logs. Fall back to 0 (console-only) if it is missing.
            var systemUserId = await context.Users
                .Where(u => u.Username == DataSeeder.SystemUsername)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync(stoppingToken);

            // Ping all devices concurrently for a fast sweep.
            var pings = await Task.WhenAll(devices.Select(async d =>
            {
                var result = await _reachability.PingAsync(d.IpAddress);
                return (Device: d, result.Reachable);
            }));

            var changes = 0;
            foreach (var (device, reachable) in pings)
            {
                if (await operations.ApplyReachabilityAsync(device, reachable, systemUserId))
                    changes++;
            }

            if (changes > 0)
                _logger.LogInformation("[DeviceMonitor] Swept {Count} device(s); {Changes} status change(s).", devices.Count, changes);
        }
    }
}
