using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <summary>
    /// Scans the configured network range for live hosts, resolving their MAC addresses and
    /// cross-referencing the result against registered devices so the UI can surface new
    /// devices to add and registered devices that are no longer responding.
    /// </summary>
    public interface IDeviceDiscoveryService
    {
        Task<DiscoveryResultDto> DiscoverAsync(int requestingUserId);

        /// <summary>
        /// Scans the configured range and returns only live hosts that are not registered.
        /// Performs no logging or persistence; intended for the background monitor.
        /// </summary>
        Task<IReadOnlyList<DiscoveredDeviceDto>> DetectUnknownAsync(CancellationToken cancellationToken = default);
    }
}
