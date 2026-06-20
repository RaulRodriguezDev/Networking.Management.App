namespace Network.Management.App.Repository.Models
{
    /// <summary>A basic static route configured on a router device.</summary>
    public class StaticRoute
    {
        public int RouteId { get; set; }

        /// <summary>Destination network, e.g. "10.0.0.0" (subnet mask is inferred for non-technical users).</summary>
        public required string Destination { get; set; }

        /// <summary>Next-hop gateway IP address.</summary>
        public required string Gateway { get; set; }

        public string? Description { get; set; }

        /// <summary>The router this route is configured on.</summary>
        public int DeviceId { get; set; }
        public Device Device { get; set; } = null!;
    }
}
