using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        [MinLength(8)]
        public required string Password { get; set; }

        [Required]
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public int RoleId { get; set; }

        [MinLength(8)]
        public string? Password { get; set; }
    }
}
