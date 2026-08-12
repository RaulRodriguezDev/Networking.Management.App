namespace Network.Management.App.Models.DTOs
{
    /// <summary>
    /// Aggregated, read-only analytics for the Reports page. Everything the page needs is
    /// returned in a single payload to keep it to one request.
    /// </summary>
    public class ReportOverviewDto
    {
        public DeviceSummaryDto DeviceSummary { get; set; } = new();
        public List<CountItemDto> DevicesByType { get; set; } = [];
        public List<CountItemDto> DevicesByLocation { get; set; } = [];
        public List<CountItemDto> DevicesByVlan { get; set; } = [];
        public List<CountItemDto> ActivityByCategory { get; set; } = [];
        public List<CountItemDto> TopActions { get; set; } = [];
        public int RecentStatusChangeCount { get; set; }
        public List<StatusChangeLogDto> RecentStatusChanges { get; set; } = [];
        public DateTime GeneratedAtUtc { get; set; }
    }

    public class DeviceSummaryDto
    {
        public int Total { get; set; }
        public int Online { get; set; }
        public int Offline { get; set; }
        public int Unregistered { get; set; }
        public double AvailabilityPercent { get; set; }
    }

    public class CountItemDto
    {
        public required string Label { get; set; }
        public int Count { get; set; }
    }

    public class StatusChangeLogDto
    {
        public DateTime CreatedAt { get; set; }
        public string? DeviceName { get; set; }
        public required string Details { get; set; }
    }
}
