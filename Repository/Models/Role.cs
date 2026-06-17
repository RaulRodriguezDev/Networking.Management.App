namespace Network.Management.App.Repository.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public string? Description { get; set; }

        public ICollection<User> Users { get; set; } = [];
    }
}
