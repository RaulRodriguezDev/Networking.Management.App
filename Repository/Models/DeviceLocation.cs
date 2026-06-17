namespace Network.Management.App.Repository.Models
{
    public class DeviceLocation
    {
        public int Id { get; set; }
        public required string LocationName { get; set; }
        public required string LocationFloor { get; set; }

        public ICollection<Device> Devices { get; set; } = [];
    }
}
