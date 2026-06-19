using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<DeviceDto>> GetAllAsync();

        Task<IEnumerable<DeviceDto>> GetByLocationAsync(int locationId);

        Task<DeviceDto?> GetByIdAsync(int deviceId);

        Task<bool> IpAddressExistsAsync(string ipAddress, int? excludeId = null);

        /// <summary>
        /// Validates that the IP is a well-formed IPv4 address and falls strictly inside the
        /// configured network range (the first and last addresses are reserved). Returns an
        /// error message, or null when the IP is acceptable.
        /// </summary>
        Task<string?> ValidateIpInRangeAsync(string ipAddress);

        /// <summary>Suggests the next free, assignable IP within the configured range, or null when full.</summary>
        Task<string?> SuggestNextIpAsync();

        Task<bool> DeviceTypeExistsAsync(int deviceTypeId);

        Task<bool> DeviceLocationExistsAsync(int deviceLocationId);

        Task<DeviceDto> CreateAsync(CreateDeviceDto dto, int requestingUserId);

        Task<DeviceDto?> UpdateAsync(int deviceId, UpdateDeviceDto dto, int requestingUserId);

        /// <summary>
        /// Connects the device to the network (future: provisions it over SSH) and marks it Online.
        /// </summary>
        Task<DeviceDto?> ConnectAsync(int deviceId, int requestingUserId);

        Task<bool> DeleteAsync(int deviceId, int requestingUserId);
    }
}
