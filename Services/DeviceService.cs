using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class DeviceService : IDeviceService
    {
        private const string DefaultStatus = DeviceStatus.Offline;
        private const int DefaultPortCount = 8;

        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;
        private readonly INetworkSettingService _networkSettingService;

        public DeviceService(AppDbContext context, IAppLogger logger, INetworkSettingService networkSettingService)
        {
            _context = context;
            _logger = logger;
            _networkSettingService = networkSettingService;
        }

        public async Task<IEnumerable<DeviceDto>> GetAllAsync()
        {
            return await _context.Devices
                .Select(d => new DeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    MacAddress = d.MacAddress,
                    Status = d.Status,
                    DeviceTypeId = d.DeviceTypeId,
                    DeviceType = d.DeviceType.TypeName,
                    DeviceLocationId = d.DeviceLocationId,
                    DeviceLocation = d.DeviceLocation.LocationName
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DeviceDto>> GetByLocationAsync(int locationId)
        {
            return await _context.Devices
                .Where(d => d.DeviceLocationId == locationId)
                .Select(d => new DeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    MacAddress = d.MacAddress,
                    Status = d.Status,
                    DeviceTypeId = d.DeviceTypeId,
                    DeviceType = d.DeviceType.TypeName,
                    DeviceLocationId = d.DeviceLocationId,
                    DeviceLocation = d.DeviceLocation.LocationName
                })
                .ToListAsync();
        }

        public async Task<DeviceDto?> GetByIdAsync(int deviceId)
        {
            return await _context.Devices
                .Where(d => d.DeviceId == deviceId)
                .Select(d => new DeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    MacAddress = d.MacAddress,
                    Status = d.Status,
                    DeviceTypeId = d.DeviceTypeId,
                    DeviceType = d.DeviceType.TypeName,
                    DeviceLocationId = d.DeviceLocationId,
                    DeviceLocation = d.DeviceLocation.LocationName
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IpAddressExistsAsync(string ipAddress, int? excludeId = null)
        {
            var normalized = ipAddress.Trim();
            return await _context.Devices
                .AnyAsync(d => d.IpAddress == normalized && (excludeId == null || d.DeviceId != excludeId));
        }

        public async Task<string?> ValidateIpInRangeAsync(string ipAddress)
        {
            if (!IpAddressHelper.TryParse(ipAddress, out var ip))
                return "IP address must be a valid IPv4 address in the format 0.0.0.0 (no subnet mask).";

            var range = await _networkSettingService.GetAsync();

            if (!IpAddressHelper.TryParse(range.RangeStart, out var start)
                || !IpAddressHelper.TryParse(range.RangeEnd, out var end))
                return "The configured network range is invalid. Please update it before adding devices.";

            // The first and last addresses of the range are reserved and cannot be assigned.
            if (ip <= start || ip >= end)
                return $"IP address must be within the assignable range ({IpAddressHelper.ToIp(start + 1)} � {IpAddressHelper.ToIp(end - 1)}).";

            return null;
        }

        public async Task<string?> SuggestNextIpAsync()
        {
            var range = await _networkSettingService.GetAsync();

            if (!IpAddressHelper.TryParse(range.RangeStart, out var start)
                || !IpAddressHelper.TryParse(range.RangeEnd, out var end))
                return null;

            var used = await _context.Devices
                .Select(d => d.IpAddress)
                .ToListAsync();

            var usedValues = new HashSet<uint>();
            foreach (var ip in used)
            {
                if (IpAddressHelper.TryParse(ip, out var value))
                    usedValues.Add(value);
            }

            // Walk the assignable range (boundaries excluded) for the first free address.
            for (var candidate = start + 1; candidate < end; candidate++)
            {
                if (!usedValues.Contains(candidate))
                    return IpAddressHelper.ToIp(candidate);
            }

            return null;
        }



        public async Task<bool> DeviceTypeExistsAsync(int deviceTypeId)
        {
            return await _context.DeviceTypes.AnyAsync(t => t.Id == deviceTypeId);
        }

        public async Task<bool> DeviceLocationExistsAsync(int deviceLocationId)
        {
            return await _context.DeviceLocations.AnyAsync(l => l.Id == deviceLocationId);
        }

        // Store MAC addresses in a single canonical form (uppercase, colon-separated) so
        // values from manual entry and discovery compare cleanly.
        private static string? NormalizeMac(string? mac)
        {
            if (string.IsNullOrWhiteSpace(mac)) return null;
            return mac.Trim().Replace('-', ':').ToUpperInvariant();
        }

        public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto, int requestingUserId)
        {
            var device = new Device
            {
                DeviceName = string.IsNullOrWhiteSpace(dto.DeviceName) ? null : dto.DeviceName.Trim(),
                IpAddress = dto.IpAddress.Trim(),
                MacAddress = NormalizeMac(dto.MacAddress),
                Status = DefaultStatus,
                DeviceTypeId = dto.DeviceTypeId,
                DeviceLocationId = dto.DeviceLocationId
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            // The device is registered as Offline. It is provisioned on the network only when
            // explicitly connected (see ConnectAsync).

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceCreated,
                $"Device '{device.DeviceName ?? device.IpAddress}' created.",
                requestingUserId,
                device.DeviceId);

            return (await GetByIdAsync(device.DeviceId))!;
        }

        public async Task<DeviceDto?> UpdateAsync(int deviceId, UpdateDeviceDto dto, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            device.DeviceName = string.IsNullOrWhiteSpace(dto.DeviceName) ? null : dto.DeviceName.Trim();
            device.IpAddress = dto.IpAddress.Trim();
            device.MacAddress = NormalizeMac(dto.MacAddress);
            device.DeviceTypeId = dto.DeviceTypeId;
            device.DeviceLocationId = dto.DeviceLocationId;

            await _context.SaveChangesAsync();

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceUpdated,
                $"Device '{device.DeviceName ?? device.IpAddress}' updated.",
                requestingUserId,
                device.DeviceId);

            return await GetByIdAsync(deviceId);
        }

        public async Task<DeviceDto?> ConnectAsync(int deviceId, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            // TODO: Establish an SSH session to the target switch/router and apply the device's
            // IP configuration here (future work). Only mark the device Online once that succeeds.

            device.Status = DeviceStatus.Online;

            // Provision a default set of physical ports the first time the device comes online,
            // so port configuration is available. TODO: replace with the real port inventory read over SSH.
            if (!await _context.Ports.AnyAsync(p => p.DeviceId == deviceId))
            {
                for (var i = 1; i <= DefaultPortCount; i++)
                {
                    _context.Ports.Add(new Port
                    {
                        DeviceId = deviceId,
                        Name = $"Port {i}",
                        IsEnabled = true,
                        IsUp = i == 1 // uplink port shows a link by default
                    });
                }
            }

            await _context.SaveChangesAsync();

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceStatusChanged,
                $"Device '{device.DeviceName ?? device.IpAddress}' connected to the network.",
                requestingUserId,
                device.DeviceId);

            return await GetByIdAsync(deviceId);
        }

        public async Task<bool> DeleteAsync(int deviceId, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(
                LogActionType.DeviceDeleted,
                $"Device '{device.DeviceName ?? device.IpAddress}' deleted.",
                requestingUserId);

            return true;
        }
    }
}
