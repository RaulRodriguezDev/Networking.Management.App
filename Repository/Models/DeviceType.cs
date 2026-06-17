namespace Network.Management.App.Repository.Models
{
    public class DeviceType
    {
        public int Id { get; set; }
        public required string TypeName { get; set; }
        public string? Description { get; set; }

        public ICollection<Device> Devices { get; set; } = [];
    }
}
