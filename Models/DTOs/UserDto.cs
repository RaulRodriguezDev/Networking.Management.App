namespace Network.Management.App.Models.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Username { get; set; }
        public required string RoleName { get; set; }
    }
}
