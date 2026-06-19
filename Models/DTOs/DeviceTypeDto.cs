namespace Network.Management.App.Models.DTOs
{
    public class DeviceTypeDto
    {
        public int Id { get; set; }
        public required string TypeName { get; set; }
        public string? Description { get; set; }
        public int DeviceCount { get; set; }
    }
}
