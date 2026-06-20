using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    /// <summary>
    /// High-level operations performed against an individual device. Configuration actions
    /// (IP, VLAN, ports, routing) are persisted to the database; the SSH/CLI push is marked
    /// with TODOs for future work. Reachability is a real ICMP ping.
    /// </summary>
    public interface IDeviceOperationsService
    {
        Task<ProbeResultDto?> ProbeAsync(int deviceId, int requestingUserId);

        /// <summary>
        /// Updates a device's stored status from a live reachability result, logging only when
        /// the status actually changes. Returns true when a change occurred.
        /// </summary>
        Task<bool> ApplyReachabilityAsync(Device device, bool reachable, int requestingUserId);

        Task<DeviceStatusDto?> GetStatusAsync(int deviceId);

        Task<DeviceDto?> ChangeIpAsync(int deviceId, string ipAddress, int requestingUserId);

        Task<DeviceDto?> AssignVlanAsync(int deviceId, int? vlanId, int requestingUserId);

        Task<DeviceDto?> MoveAsync(int deviceId, int locationId, int requestingUserId);

        // ===== Ports =====
        Task<IEnumerable<PortDto>> GetPortsAsync(int deviceId);

        Task<PortDto?> ConfigurePortAsync(int deviceId, int portId, ConfigurePortDto dto, int requestingUserId);

        // ===== Routing =====
        Task<IEnumerable<RouteDto>> GetRoutesAsync(int deviceId);

        Task<RouteDto?> AddRouteAsync(int deviceId, SaveRouteDto dto, int requestingUserId);

        Task<bool> DeleteRouteAsync(int deviceId, int routeId, int requestingUserId);
    }
}
