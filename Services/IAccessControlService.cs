using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public static class AccessMode
    {
        public const string All = "All";
        public const string Restricted = "Restricted";
    }

    public interface IAccessControlService
    {
        /// <summary>Returns the current access settings (DB) plus the configured allowlist (web.config).</summary>
        Task<AccessSettingDto> GetAsync();

        /// <summary>Updates the admin-editable access settings (mode + master IP). Takes effect immediately.</summary>
        Task<AccessSettingDto> UpdateAsync(UpdateAccessSettingDto dto, int requestingUserId);

        /// <summary>
        /// Decides whether the given client IP may access the app. Localhost is always allowed.
        /// </summary>
        Task<bool> IsAllowedAsync(string? clientIp);
    }
}
