using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<LocationDto>> GetAllAsync();

        Task<LocationDto?> GetByIdAsync(int locationId);

        Task<bool> LocationNameExistsAsync(string locationName, int? excludeId = null);

        Task<bool> HasDevicesAsync(int locationId);

        Task<LocationDto> CreateAsync(CreateLocationDto dto, int requestingUserId);

        Task<LocationDto?> UpdateAsync(int locationId, UpdateLocationDto dto, int requestingUserId);

        Task<bool> DeleteAsync(int locationId, int requestingUserId);
    }
}
