using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    public class CreateDeviceDto
    {
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [Required]
        [MaxLength(45)]
        public required string IpAddress { get; set; }

        [MaxLength(17)]
        public string? MacAddress { get; set; }

        [Required]
        public int DeviceTypeId { get; set; }

        [Required]
        public int DeviceLocationId { get; set; }
    }

    public class UpdateDeviceDto
    {
        [MaxLength(100)]
        public string? DeviceName { get; set; }

        [Required]
        [MaxLength(45)]
        public required string IpAddress { get; set; }

        [MaxLength(17)]
        public string? MacAddress { get; set; }

        [Required]
        public int DeviceTypeId { get; set; }

        [Required]
        public int DeviceLocationId { get; set; }
    }

    public class CreateDeviceTypeDto
    {
        [Required]
        [MaxLength(50)]
        public required string TypeName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
