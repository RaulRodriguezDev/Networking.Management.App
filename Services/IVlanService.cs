using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface IVlanService
    {
        Task<IEnumerable<VlanDto>> GetAllAsync();

        Task<VlanDto?> GetByIdAsync(int vlanId);

        Task<bool> VlanNumberExistsAsync(int vlanNumber, int? excludeId = null);

        Task<VlanDto> CreateAsync(SaveVlanDto dto, int requestingUserId);

        Task<VlanDto?> UpdateAsync(int vlanId, SaveVlanDto dto, int requestingUserId);

        Task<bool> DeleteAsync(int vlanId, int requestingUserId);
    }
}
