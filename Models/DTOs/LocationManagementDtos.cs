using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    public class CreateLocationDto
    {
        [Required]
        [MaxLength(100)]
        public required string LocationName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LocationFloor { get; set; }
    }

    public class UpdateLocationDto
    {
        [Required]
        [MaxLength(100)]
        public required string LocationName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LocationFloor { get; set; }
    }
}
