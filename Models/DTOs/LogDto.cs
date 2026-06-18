namespace Network.Management.App.Models.DTOs
{
    public class LogDto
    {
        public int LogId { get; set; }
        public required string ActionType { get; set; }
        public required string Details { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string Category { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public int? DeviceId { get; set; }
        public string? DeviceName { get; set; }
    }
}
