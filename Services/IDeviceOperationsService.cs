using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <summary>
    /// High-level operations performed against an individual device. Network actions are
    /// currently simulated — the SSH/CLI integration is marked with TODOs for future work.
    /// </summary>
    public interface IDeviceOperationsService
    {
        Task<ProbeResultDto?> ProbeAsync(int deviceId, int requestingUserId);

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
