using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class NetworkSettingService : INetworkSettingService
    {
        private const string DefaultRangeStart = "192.168.1.1";
        private const string DefaultRangeEnd = "192.168.1.254";

        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public NetworkSettingService(AppDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NetworkSettingDto> GetAsync()
        {
            var setting = await _context.NetworkSettings.FirstOrDefaultAsync();

            return new NetworkSettingDto
            {
                RangeStart = setting?.RangeStart ?? DefaultRangeStart,
                RangeEnd = setting?.RangeEnd ?? DefaultRangeEnd
            };
        }

        public async Task<NetworkSettingDto> UpdateAsync(UpdateNetworkSettingDto dto, int requestingUserId)
        {
            var setting = await _context.NetworkSettings.FirstOrDefaultAsync();
            var previous = setting is null ? $"{DefaultRangeStart} - {DefaultRangeEnd}" : $"{setting.RangeStart} - {setting.RangeEnd}";

            if (setting is null)
            {
                setting = new NetworkSetting
                {
                    RangeStart = dto.RangeStart.Trim(),
                    RangeEnd = dto.RangeEnd.Trim()
                };
                _context.NetworkSettings.Add(setting);
            }
            else
            {
                setting.RangeStart = dto.RangeStart.Trim();
                setting.RangeEnd = dto.RangeEnd.Trim();
            }

            await _context.SaveChangesAsync();

            // TODO: Reconfigure the network equipment (DHCP scope / switch ACLs) over SSH (future work).

            await _logger.PersistAsync(
                LogActionType.NetworkRangeChanged,
                $"Network IP range changed from '{previous}' to '{setting.RangeStart} - {setting.RangeEnd}'.",
                requestingUserId);

            return new NetworkSettingDto
            {
                RangeStart = setting.RangeStart,
                RangeEnd = setting.RangeEnd
            };
        }
    }
}
