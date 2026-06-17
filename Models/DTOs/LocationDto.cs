namespace Network.Management.App.Models.DTOs
{
    public class LocationDto
    {
        public int Id { get; set; }
        public required string LocationName { get; set; }
        public required string LocationFloor { get; set; }
        public int TotalDevices { get; set; }
        public int OnlineDevices { get; set; }
        public int OfflineDevices { get; set; }
        public required string Status { get; set; }
    }
}
