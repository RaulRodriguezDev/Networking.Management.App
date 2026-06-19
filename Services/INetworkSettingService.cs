using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface INetworkSettingService
    {
        /// <summary>Returns the configured IP range, or a default range when none has been set yet.</summary>
        Task<NetworkSettingDto> GetAsync();

        /// <summary>Updates the network IP range. This is a network-wide change.</summary>
        Task<NetworkSettingDto> UpdateAsync(UpdateNetworkSettingDto dto, int requestingUserId);
    }
}
