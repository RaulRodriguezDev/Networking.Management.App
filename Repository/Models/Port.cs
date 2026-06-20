namespace Network.Management.App.Repository.Models
{
    /// <summary>A physical port on a switch or router (a network device).</summary>
    public class Port
    {
        public int PortId { get; set; }

        /// <summary>Human-friendly port label, e.g. "GigabitEthernet0/1".</summary>
        public required string Name { get; set; }

        /// <summary>Whether the port is administratively enabled.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>Whether a link is currently detected on the port.</summary>
        public bool IsUp { get; set; }

        /// <summary>The device (switch/router) this port belongs to.</summary>
        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;

        /// <summary>The VLAN assigned to this access port, if any.</summary>
        public int? VlanId { get; set; }
        public Vlan? Vlan { get; set; }
    }
}
