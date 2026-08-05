using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <inheritdoc />
    public class StatusChangeTracker : IStatusChangeTracker
    {
        // Cap the retained history so the tracker stays bounded on a busy network.
        private const int MaxRetained = 50;

        private readonly object _gate = new();
        private readonly LinkedList<StatusChangeDto> _changes = new();
        private long _nextId;

        public StatusChangeDto Record(int deviceId, string deviceName, string ipAddress, string previousStatus, string newStatus)
        {
            lock (_gate)
            {
                var entry = new StatusChangeDto
                {
                    Id = ++_nextId,
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    IpAddress = ipAddress,
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus,
                    ChangedUtc = DateTime.UtcNow
                };

                _changes.AddFirst(entry);
                while (_changes.Count > MaxRetained)
                    _changes.RemoveLast();

                return entry;
            }
        }

        public StatusChangesDto Snapshot()
        {
            lock (_gate)
            {
                var changes = _changes.ToList();
                return new StatusChangesDto
                {
                    Count = changes.Count,
                    LatestId = _nextId,
                    Changes = changes
                };
            }
        }

        public void Dismiss(long id)
        {
            lock (_gate)
            {
                var node = _changes.First;
                while (node is not null)
                {
                    if (node.Value.Id == id)
                    {
                        _changes.Remove(node);
                        return;
                    }
                    node = node.Next;
                }
            }
        }

        public void DismissAll()
        {
            lock (_gate)
            {
                _changes.Clear();
            }
        }
    }
}
