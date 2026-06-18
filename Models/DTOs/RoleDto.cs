namespace Network.Management.App.Models.DTOs
{
    public class RoleDto
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public string? Description { get; set; }
    }
}
