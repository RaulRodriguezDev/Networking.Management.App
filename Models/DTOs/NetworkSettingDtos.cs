using System.ComponentModel.DataAnnotations;

namespace Network.Management.App.Models.DTOs
{
    public class NetworkSettingDto
    {
        public required string RangeStart { get; set; }
        public required string RangeEnd { get; set; }
    }

    public class UpdateNetworkSettingDto
    {
        [Required]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Range start must be a valid IPv4 address.")]
        public required string RangeStart { get; set; }

        [Required]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Range end must be a valid IPv4 address.")]
        public required string RangeEnd { get; set; }
    }
}
