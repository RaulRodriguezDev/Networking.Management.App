using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    /// <inheritdoc />
    public class UnknownDeviceTracker : IUnknownDeviceTracker
    {
        private readonly object _gate = new();
        private Dictionary<string, UnknownDeviceDto> _current = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _dismissed = new(StringComparer.OrdinalIgnoreCase);
        private DateTime? _lastScanUtc;

        public IReadOnlyList<UnknownDeviceDto> Update(IEnumerable<DiscoveredDeviceDto> unknownHosts)
        {
            var now = DateTime.UtcNow;
            var newly = new List<UnknownDeviceDto>();

            lock (_gate)
            {
                var next = new Dictionary<string, UnknownDeviceDto>(StringComparer.OrdinalIgnoreCase);

                foreach (var host in unknownHosts)
                {
                    if (_current.TryGetValue(host.IpAddress, out var existing))
                    {
                        // Preserve the original first-seen time; refresh identity details.
                        existing.MacAddress = host.MacAddress ?? existing.MacAddress;
                        existing.HostName = host.HostName ?? existing.HostName;
                        next[host.IpAddress] = existing;
                    }
                    else
                    {
                        var added = new UnknownDeviceDto
                        {
                            IpAddress = host.IpAddress,
                            MacAddress = host.MacAddress,
                            HostName = host.HostName,
                            FirstSeenUtc = now
                        };
                        next[host.IpAddress] = added;
                        newly.Add(added);
                    }
                }

                // Forget dismissals for hosts that have left the network, so a device that
                // disappears and later returns will raise a fresh alert.
                _dismissed.IntersectWith(next.Keys);

                _current = next;
                _lastScanUtc = now;
            }

            return newly;
        }

        public UnknownDevicesDto Snapshot()
        {
            lock (_gate)
            {
                var active = _current.Values
                    .Where(d => !_dismissed.Contains(d.IpAddress))
                    .OrderBy(d => IpAddressHelper.TryParse(d.IpAddress, out var v) ? v : uint.MaxValue)
                    .Select(d => new UnknownDeviceDto
                    {
                        IpAddress = d.IpAddress,
                        MacAddress = d.MacAddress,
                        HostName = d.HostName,
                        FirstSeenUtc = d.FirstSeenUtc
                    })
                    .ToList();

                return new UnknownDevicesDto
                {
                    Count = active.Count,
                    LastScanUtc = _lastScanUtc,
                    Devices = active
                };
            }
        }

        public void Dismiss(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return;
            lock (_gate)
            {
                if (_current.ContainsKey(ipAddress))
                    _dismissed.Add(ipAddress);
            }
        }

        public void DismissAll()
        {
            lock (_gate)
            {
                foreach (var ip in _current.Keys)
                    _dismissed.Add(ip);
            }
        }

        public void Remove(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return;
            lock (_gate)
            {
                _current.Remove(ipAddress);
                _dismissed.Remove(ipAddress);
            }
        }
    }
}
