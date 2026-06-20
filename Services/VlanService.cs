using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class VlanService : IVlanService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public VlanService(AppDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<VlanDto>> GetAllAsync()
        {
            return await _context.Vlans
                .OrderBy(v => v.VlanNumber)
                .Select(v => new VlanDto
                {
                    VlanId = v.VlanId,
                    VlanNumber = v.VlanNumber,
                    Name = v.Name,
                    Description = v.Description,
                    DeviceCount = v.Devices.Count
                })
                .ToListAsync();
        }

        public async Task<VlanDto?> GetByIdAsync(int vlanId)
        {
            return await _context.Vlans
                .Where(v => v.VlanId == vlanId)
                .Select(v => new VlanDto
                {
                    VlanId = v.VlanId,
                    VlanNumber = v.VlanNumber,
                    Name = v.Name,
                    Description = v.Description,
                    DeviceCount = v.Devices.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> VlanNumberExistsAsync(int vlanNumber, int? excludeId = null)
        {
            return await _context.Vlans
                .AnyAsync(v => v.VlanNumber == vlanNumber && (excludeId == null || v.VlanId != excludeId));
        }

        public async Task<VlanDto> CreateAsync(SaveVlanDto dto, int requestingUserId)
        {
            var vlan = new Vlan
            {
                VlanNumber = dto.VlanNumber,
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };

            _context.Vlans.Add(vlan);
            await _context.SaveChangesAsync();

            // TODO: Push the VLAN definition to the managed switches over SSH (future work).

            await _logger.PersistAsync(
                LogActionType.VlanCreated,
                $"VLAN {vlan.VlanNumber} ('{vlan.Name}') created.",
                requestingUserId);

            return (await GetByIdAsync(vlan.VlanId))!;
        }

        public async Task<VlanDto?> UpdateAsync(int vlanId, SaveVlanDto dto, int requestingUserId)
        {
            var vlan = await _context.Vlans.FindAsync(vlanId);
            if (vlan is null) return null;

            vlan.VlanNumber = dto.VlanNumber;
            vlan.Name = dto.Name.Trim();
            vlan.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

            await _context.SaveChangesAsync();

            // TODO: Update the VLAN definition on the managed switches over SSH (future work).

            await _logger.PersistAsync(
                LogActionType.VlanUpdated,
                $"VLAN {vlan.VlanNumber} ('{vlan.Name}') updated.",
                requestingUserId);

            return await GetByIdAsync(vlanId);
        }

        public async Task<bool> DeleteAsync(int vlanId, int requestingUserId)
        {
            var vlan = await _context.Vlans.FindAsync(vlanId);
            if (vlan is null) return false;

            _context.Vlans.Remove(vlan);
            await _context.SaveChangesAsync();

            // TODO: Remove the VLAN from the managed switches over SSH (future work).

            await _logger.PersistAsync(
                LogActionType.VlanDeleted,
                $"VLAN {vlan.VlanNumber} ('{vlan.Name}') deleted.",
                requestingUserId);

            return true;
        }
    }
}
