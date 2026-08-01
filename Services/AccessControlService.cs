using System.Net;
using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class AccessControlService : IAccessControlService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;
        private readonly IConfiguration _configuration;

        public AccessControlService(AppDbContext context, IAppLogger logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<AccessSettingDto> GetAsync()
        {
            var setting = await _context.AccessSettings.FirstOrDefaultAsync();

            return new AccessSettingDto
            {
                Mode = setting?.Mode ?? AccessMode.All,
                MasterIp = setting?.MasterIp,
                ConfiguredAllowList = ReadConfiguredAllowList()
            };
        }

        public async Task<AccessSettingDto> UpdateAsync(UpdateAccessSettingDto dto, int requestingUserId)
        {
            var setting = await _context.AccessSettings.FirstOrDefaultAsync();
            var previousMode = setting?.Mode ?? AccessMode.All;
            var previousIp = setting?.MasterIp ?? "(none)";

            var masterIp = string.IsNullOrWhiteSpace(dto.MasterIp) ? null : dto.MasterIp.Trim();

            if (setting is null)
            {
                setting = new AccessSetting { Mode = dto.Mode, MasterIp = masterIp };
                _context.AccessSettings.Add(setting);
            }
            else
            {
                setting.Mode = dto.Mode;
                setting.MasterIp = masterIp;
            }

            await _context.SaveChangesAsync();

            await _logger.PersistAsync(
                LogActionType.AccessSettingChanged,
                $"Access control changed from [{previousMode}, master {previousIp}] to [{setting.Mode}, master {setting.MasterIp ?? "(none)"}].",
                requestingUserId);

            return new AccessSettingDto
            {
                Mode = setting.Mode,
                MasterIp = setting.MasterIp,
                ConfiguredAllowList = ReadConfiguredAllowList()
            };
        }

        public async Task<bool> IsAllowedAsync(string? clientIp)
        {
            if (!IPAddress.TryParse(clientIp, out var ip))
                return false;

            // The server itself (loopback) is always allowed so an admin can never be locked out locally.
            if (IPAddress.IsLoopback(ip))
                return true;

            var normalized = NormalizeIp(ip);

            // Configuration allowlist (web.config / appsettings). "All" or empty => unrestricted.
            var configured = ReadConfiguredAllowList().ToList();
            if (configured.Any(entry => string.Equals(entry, AccessMode.All, StringComparison.OrdinalIgnoreCase)))
                return true;
            if (configured.Contains(normalized))
                return true;

            // Admin-editable database setting (takes effect immediately, no redeploy).
            var setting = await _context.AccessSettings.AsNoTracking().FirstOrDefaultAsync();
            var mode = setting?.Mode ?? AccessMode.All;

            if (string.Equals(mode, AccessMode.All, StringComparison.OrdinalIgnoreCase))
                return true;

            return !string.IsNullOrWhiteSpace(setting?.MasterIp)
                && string.Equals(setting!.MasterIp.Trim(), normalized, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the configured allowlist from AccessControl:AllowedIps. Defaults to ["All"]
        /// (unrestricted) when the section is missing, matching the requested initial behavior.
        /// </summary>
        private IEnumerable<string> ReadConfiguredAllowList()
        {
            var values = _configuration.GetSection("AccessControl:AllowedIps").Get<string[]>();

            if (values is null || values.Length == 0)
                return [AccessMode.All];

            return values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v.Trim());
        }

        // Map IPv4-mapped IPv6 addresses (e.g. ::ffff:192.168.1.5) back to plain IPv4 for comparison.
        private static string NormalizeIp(IPAddress ip)
        {
            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();
            return ip.ToString();
        }
    }
}
