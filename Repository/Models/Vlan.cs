namespace Network.Management.App.Repository.Models
{
    public class Vlan
    {
        public int VlanId { get; set; }

        /// <summary>The 802.1Q VLAN number (1-4094).</summary>
        public int VlanNumber { get; set; }

        public required string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<Device> Devices { get; set; } = [];
    }
}
