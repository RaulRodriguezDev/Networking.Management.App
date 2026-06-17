namespace Network.Management.App.Repository.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public required string ActionType { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string Details { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
