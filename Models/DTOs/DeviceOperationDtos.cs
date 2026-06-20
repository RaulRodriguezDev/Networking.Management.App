using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    // ===== VLANs =====
    public class VlanDto
    {
        public int VlanId { get; set; }
        public int VlanNumber { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int DeviceCount { get; set; }
    }

    public class SaveVlanDto
    {
        [Required]
        [Range(1, 4094, ErrorMessage = "VLAN number must be between 1 and 4094.")]
        public int VlanNumber { get; set; }

        [Required]
        [MaxLength(60)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    // ===== Ports =====
    public class PortDto
    {
        public int PortId { get; set; }
        public required string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsUp { get; set; }
        public int? VlanId { get; set; }
        public int? VlanNumber { get; set; }
        public string? VlanName { get; set; }
    }

    public class ConfigurePortDto
    {
        [Required]
        public bool IsEnabled { get; set; }

        /// <summary>VLAN to assign to this access port, or null to clear it.</summary>
        public int? VlanId { get; set; }
    }

    // ===== Operations on a device =====
    public class ProbeResultDto
    {
        public bool Success { get; set; }
        public required string Message { get; set; }

        /// <summary>Round-trip time in milliseconds when the probe succeeds.</summary>
        public int? LatencyMs { get; set; }
    }

    public class DeviceStatusDto
    {
        public int DeviceId { get; set; }
        public required string Status { get; set; }
        public required string IpAddress { get; set; }
        public int? VlanId { get; set; }
        public int? VlanNumber { get; set; }
        public string? VlanName { get; set; }
        public int PortCount { get; set; }
        public int PortsUp { get; set; }
    }

    public class ChangeDeviceIpDto
    {
        [Required]
        [MaxLength(45)]
        public required string IpAddress { get; set; }
    }

    public class AssignVlanDto
    {
        /// <summary>VLAN to attach the device to, or null to detach.</summary>
        public int? VlanId { get; set; }
    }

    public class MoveDeviceDto
    {
        [Required]
        public int DeviceLocationId { get; set; }
    }

    // ===== Routing (basic static routes) =====
    public class RouteDto
    {
        public int RouteId { get; set; }
        public required string Destination { get; set; }
        public required string Gateway { get; set; }
        public string? Description { get; set; }
    }

    public class SaveRouteDto
    {
        [Required]
        [MaxLength(45)]
        public required string Destination { get; set; }

        [Required]
        [MaxLength(45)]
        public required string Gateway { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
