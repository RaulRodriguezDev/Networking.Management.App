using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface IDeviceTypeService
    {
        Task<IEnumerable<DeviceTypeDto>> GetAllAsync();

        Task<DeviceTypeDto?> GetByIdAsync(int deviceTypeId);

        Task<bool> TypeNameExistsAsync(string typeName, int? excludeId = null);

        Task<DeviceTypeDto> CreateAsync(CreateDeviceTypeDto dto, int requestingUserId);
    }
}
