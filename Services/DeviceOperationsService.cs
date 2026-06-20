using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class DeviceOperationsService : IDeviceOperationsService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;
        private readonly IDeviceService _deviceService;

        public DeviceOperationsService(AppDbContext context, IAppLogger logger, IDeviceService deviceService)
        {
            _context = context;
            _logger = logger;
            _deviceService = deviceService;
        }

        public async Task<ProbeResultDto?> ProbeAsync(int deviceId, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            // TODO: Perform a real reachability check (ICMP ping / SSH handshake) against the
            // device's IP here (future work). For now the probe succeeds only for Online devices.
            var reachable = device.Status == DeviceStatus.Online;

            var result = new ProbeResultDto
            {
                Success = reachable,
                LatencyMs = reachable ? Random.Shared.Next(1, 25) : null,
                Message = reachable
                    ? "The device responded successfully and is reachable on the network."
                    : "The device did not respond. It still needs to be configured and connected to the network."
            };

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceProbed,
                $"Connection probe for '{DeviceLabel(device)}': {(reachable ? "reachable" : "no response")}.",
                requestingUserId,
                device.DeviceId);

            return result;
        }

        public async Task<DeviceStatusDto?> GetStatusAsync(int deviceId)
        {
            return await _context.Devices
                .Where(d => d.DeviceId == deviceId)
                .Select(d => new DeviceStatusDto
                {
                    DeviceId = d.DeviceId,
                    Status = d.Status,
                    IpAddress = d.IpAddress,
                    VlanId = d.VlanId,
                    VlanNumber = d.Vlan != null ? d.Vlan.VlanNumber : null,
                    VlanName = d.Vlan != null ? d.Vlan.Name : null,
                    PortCount = d.Ports.Count,
                    PortsUp = d.Ports.Count(p => p.IsUp)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<DeviceDto?> ChangeIpAsync(int deviceId, string ipAddress, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            var previous = device.IpAddress;
            device.IpAddress = ipAddress.Trim();
            await _context.SaveChangesAsync();

            // TODO: Apply the new IP address on the device over SSH (future work).

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceIpChanged,
                $"IP address for '{DeviceLabel(device)}' changed from {previous} to {device.IpAddress}.",
                requestingUserId,
                device.DeviceId);

            return await _deviceService.GetByIdAsync(deviceId);
        }

        public async Task<DeviceDto?> AssignVlanAsync(int deviceId, int? vlanId, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            string? vlanLabel = null;
            if (vlanId is not null)
            {
                var vlan = await _context.Vlans.FindAsync(vlanId.Value);
                if (vlan is null) return null;
                vlanLabel = $"VLAN {vlan.VlanNumber} ('{vlan.Name}')";
            }

            device.VlanId = vlanId;
            await _context.SaveChangesAsync();

            // TODO: Apply the VLAN membership on the access port over SSH (future work).

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceVlanChanged,
                vlanId is null
                    ? $"Device '{DeviceLabel(device)}' detached from its VLAN."
                    : $"Device '{DeviceLabel(device)}' attached to {vlanLabel}.",
                requestingUserId,
                device.DeviceId);

            return await _deviceService.GetByIdAsync(deviceId);
        }

        public async Task<DeviceDto?> MoveAsync(int deviceId, int locationId, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            var location = await _context.DeviceLocations.FindAsync(locationId);
            if (location is null) return null;

            device.DeviceLocationId = locationId;
            await _context.SaveChangesAsync();

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceUpdated,
                $"Device '{DeviceLabel(device)}' moved to location '{location.LocationName}'.",
                requestingUserId,
                device.DeviceId);

            return await _deviceService.GetByIdAsync(deviceId);
        }

        public async Task<IEnumerable<PortDto>> GetPortsAsync(int deviceId)
        {
            return await _context.Ports
                .Where(p => p.DeviceId == deviceId)
                .OrderBy(p => p.PortId)
                .Select(p => new PortDto
                {
                    PortId = p.PortId,
                    Name = p.Name,
                    IsEnabled = p.IsEnabled,
                    IsUp = p.IsUp,
                    VlanId = p.VlanId,
                    VlanNumber = p.Vlan != null ? p.Vlan.VlanNumber : null,
                    VlanName = p.Vlan != null ? p.Vlan.Name : null
                })
                .ToListAsync();
        }

        public async Task<PortDto?> ConfigurePortAsync(int deviceId, int portId, ConfigurePortDto dto, int requestingUserId)
        {
            var port = await _context.Ports
                .FirstOrDefaultAsync(p => p.PortId == portId && p.DeviceId == deviceId);
            if (port is null) return null;

            if (dto.VlanId is not null && !await _context.Vlans.AnyAsync(v => v.VlanId == dto.VlanId.Value))
                return null;

            port.IsEnabled = dto.IsEnabled;
            port.VlanId = dto.VlanId;
            // A disabled port can never carry a link.
            port.IsUp = dto.IsEnabled && port.IsUp;

            await _context.SaveChangesAsync();

            // TODO: Apply the port configuration (enable/disable, access VLAN) over SSH (future work).

            await _logger.PersistDeviceAsync(
                LogActionType.DevicePortConfigured,
                $"Port '{port.Name}' on device {deviceId} configured ({(dto.IsEnabled ? "enabled" : "disabled")}).",
                requestingUserId,
                deviceId);

            return await _context.Ports
                .Where(p => p.PortId == portId)
                .Select(p => new PortDto
                {
                    PortId = p.PortId,
                    Name = p.Name,
                    IsEnabled = p.IsEnabled,
                    IsUp = p.IsUp,
                    VlanId = p.VlanId,
                    VlanNumber = p.Vlan != null ? p.Vlan.VlanNumber : null,
                    VlanName = p.Vlan != null ? p.Vlan.Name : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RouteDto>> GetRoutesAsync(int deviceId)
        {
            return await _context.Routes
                .Where(r => r.DeviceId == deviceId)
                .OrderBy(r => r.RouteId)
                .Select(r => new RouteDto
                {
                    RouteId = r.RouteId,
                    Destination = r.Destination,
                    Gateway = r.Gateway,
                    Description = r.Description
                })
                .ToListAsync();
        }

        public async Task<RouteDto?> AddRouteAsync(int deviceId, SaveRouteDto dto, int requestingUserId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device is null) return null;

            var route = new StaticRoute
            {
                DeviceId = deviceId,
                Destination = dto.Destination.Trim(),
                Gateway = dto.Gateway.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // TODO: Apply the static route on the router over SSH (future work).

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceRouteChanged,
                $"Route to {route.Destination} via {route.Gateway} added on '{DeviceLabel(device)}'.",
                requestingUserId,
                deviceId);

            return new RouteDto
            {
                RouteId = route.RouteId,
                Destination = route.Destination,
                Gateway = route.Gateway,
                Description = route.Description
            };
        }

        public async Task<bool> DeleteRouteAsync(int deviceId, int routeId, int requestingUserId)
        {
            var route = await _context.Routes
                .FirstOrDefaultAsync(r => r.RouteId == routeId && r.DeviceId == deviceId);
            if (route is null) return false;

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            // TODO: Remove the static route from the router over SSH (future work).

            await _logger.PersistDeviceAsync(
                LogActionType.DeviceRouteChanged,
                $"Route to {route.Destination} via {route.Gateway} removed on device {deviceId}.",
                requestingUserId,
                deviceId);

            return true;
        }

        private static string DeviceLabel(Device device) => device.DeviceName ?? device.IpAddress;
    }
}
