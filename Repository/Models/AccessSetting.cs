namespace Network.Management.App.Repository.Models
{
    /// <summary>
    /// Admin-editable access control settings (single row). Lets an administrator restrict
    /// which client may reach the app to a single "master" PC, without a redeploy.
    /// </summary>
    public class AccessSetting
    {
        public int Id { get; set; }

        /// <summary>"All" to allow every IP, or "Restricted" to only allow the master IP.</summary>
        public required string Mode { get; set; }

        /// <summary>The allowed master PC IPv4 address when the mode is Restricted.</summary>
        public string? MasterIp { get; set; }
    }
}
