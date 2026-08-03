namespace Network.Management.App.Repository.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public required string IpAddress { get; set; }
        public string? MacAddress { get; set; }
        public required string Status { get; set; }

        public int DeviceTypeId { get; set; }
        public DeviceType DeviceType { get; set; } = null!;

        public int DeviceLocationId { get; set; }
        public DeviceLocation DeviceLocation { get; set; } = null!;

        public int? VlanId { get; set; }
        public Vlan? Vlan { get; set; }

        public ICollection<Port> Ports { get; set; } = [];
        public ICollection<StaticRoute> Routes { get; set; } = [];
    }
}

