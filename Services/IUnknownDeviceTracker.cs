using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <summary>
    /// Holds the current set of live-but-unregistered hosts detected by the background monitor,
    /// so the UI can alert admins to devices that are on the network but not yet added to the app.
    /// </summary>
    public interface IUnknownDeviceTracker
    {
        /// <summary>Replaces the tracked set and returns the hosts that are newly detected this scan.</summary>
        IReadOnlyList<UnknownDeviceDto> Update(IEnumerable<DiscoveredDeviceDto> unknownHosts);

        /// <summary>Current snapshot of unregistered hosts for the notifications endpoint.</summary>
        UnknownDevicesDto Snapshot();

        /// <summary>Acknowledges a single host so it no longer counts toward alerts.</summary>
        void Dismiss(string ipAddress);

        /// <summary>Acknowledges every currently tracked host.</summary>
        void DismissAll();

        /// <summary>Drops a host entirely (e.g. once it has been registered as a device).</summary>
        void Remove(string ipAddress);
    }
}
