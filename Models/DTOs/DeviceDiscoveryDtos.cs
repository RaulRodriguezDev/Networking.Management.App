namespace Network.Management.App.Models.DTOs
{
    /// <summary>
    /// A host that responded to the network scan. Transient (not persisted): it only
    /// describes what was seen on the wire so the UI can offer to register or remove it.
    /// </summary>
    public class DiscoveredDeviceDto
    {
        public required string IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public long? LatencyMs { get; set; }

        // Reverse-DNS (PTR) name seen on the network; null when the host has no PTR record.
        public string? HostName { get; set; }

        // True when a device with this IP already exists in the app.
        public bool IsRegistered { get; set; }
        public int? DeviceId { get; set; }
        public string? DeviceName { get; set; }
    }

    /// <summary>
    /// A device registered in the app that did not respond to the scan (candidate for removal).
    /// </summary>
    public class MissingDeviceDto
    {
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public required string IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public required string Status { get; set; }
    }

    public class DiscoveryResultDto
    {
        public required string RangeStart { get; set; }
        public required string RangeEnd { get; set; }
        public int ScannedCount { get; set; }
        public List<DiscoveredDeviceDto> Discovered { get; set; } = [];
        public List<MissingDeviceDto> Missing { get; set; } = [];
    }

    /// <summary>
    /// A live host seen on the network that is not registered in the app. Tracked in memory
    /// by the background monitor so the UI can alert admins to devices awaiting registration.
    /// </summary>
    public class UnknownDeviceDto
    {
        public required string IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public string? HostName { get; set; }
        public DateTime FirstSeenUtc { get; set; }
    }

    public class UnknownDevicesDto
    {
        public int Count { get; set; }
        public DateTime? LastScanUtc { get; set; }
        public List<UnknownDeviceDto> Devices { get; set; } = [];
    }

    public class DismissUnknownDto
    {
        public required string IpAddress { get; set; }
    }

    /// <summary>
    /// A registered device that transitioned between Online and Offline. Tracked in memory
    /// by the background monitor so the UI can alert admins to reachability changes.
    /// </summary>
    public class StatusChangeDto
    {
        public long Id { get; set; }
        public int DeviceId { get; set; }
        public required string DeviceName { get; set; }
        public required string IpAddress { get; set; }
        public required string PreviousStatus { get; set; }
        public required string NewStatus { get; set; }
        public DateTime ChangedUtc { get; set; }
    }

    public class StatusChangesDto
    {
        public int Count { get; set; }
        public long LatestId { get; set; }
        public List<StatusChangeDto> Changes { get; set; } = [];
    }

    public class DismissStatusChangeDto
    {
        public long Id { get; set; }
    }
}
