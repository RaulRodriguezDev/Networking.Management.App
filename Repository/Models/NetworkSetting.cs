namespace Network.Management.App.Repository.Models
{
    public class NetworkSetting
    {
        public int Id { get; set; }
        public required string RangeStart { get; set; }
        public required string RangeEnd { get; set; }
    }
}
