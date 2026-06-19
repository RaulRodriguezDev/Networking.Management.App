namespace Network.Management.App.Models.DTOs
{
    public class DeviceDto
    {
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public required string IpAddress { get; set; }
        public required string Status { get; set; }
        public int DeviceTypeId { get; set; }
        public required string DeviceType { get; set; }
        public int DeviceLocationId { get; set; }
        public required string DeviceLocation { get; set; }
    }
}
