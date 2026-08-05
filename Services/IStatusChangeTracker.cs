using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <summary>
    /// Keeps a short in-memory history of registered-device Online/Offline transitions so the
    /// UI can surface them as notifications. Shared as a singleton across the app.
    /// </summary>
    public interface IStatusChangeTracker
    {
        // Records a transition and returns the created notification entry.
        StatusChangeDto Record(int deviceId, string deviceName, string ipAddress, string previousStatus, string newStatus);

        StatusChangesDto Snapshot();

        void Dismiss(long id);

        void DismissAll();
    }
}
