using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    public class AccessSettingDto
    {
        /// <summary>"All" or "Restricted".</summary>
        public required string Mode { get; set; }

        public string? MasterIp { get; set; }

        /// <summary>The allowlist coming from configuration (web.config / appsettings); read-only here.</summary>
        public required IEnumerable<string> ConfiguredAllowList { get; set; }
    }

    public class UpdateAccessSettingDto
    {
        [Required]
        [RegularExpression("All|Restricted", ErrorMessage = "Mode must be 'All' or 'Restricted'.")]
        public required string Mode { get; set; }

        [MaxLength(45)]
        public string? MasterIp { get; set; }
    }
}
