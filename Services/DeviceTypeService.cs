using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class DeviceTypeService : IDeviceTypeService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public DeviceTypeService(AppDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<DeviceTypeDto>> GetAllAsync()
        {
            return await _context.DeviceTypes
                .Select(t => new DeviceTypeDto
                {
                    Id = t.Id,
                    TypeName = t.TypeName,
                    Description = t.Description,
                    DeviceCount = t.Devices.Count
                })
                .ToListAsync();
        }

        public async Task<DeviceTypeDto?> GetByIdAsync(int deviceTypeId)
        {
            return await _context.DeviceTypes
                .Where(t => t.Id == deviceTypeId)
                .Select(t => new DeviceTypeDto
                {
                    Id = t.Id,
                    TypeName = t.TypeName,
                    Description = t.Description,
                    DeviceCount = t.Devices.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> TypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            var normalized = typeName.Trim();
            return await _context.DeviceTypes
                .AnyAsync(t => t.TypeName == normalized && (excludeId == null || t.Id != excludeId));
        }

        public async Task<DeviceTypeDto> CreateAsync(CreateDeviceTypeDto dto, int requestingUserId)
        {
            var deviceType = new DeviceType
            {
                TypeName = dto.TypeName.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };

            _context.DeviceTypes.Add(deviceType);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(
                LogActionType.DeviceTypeCreated,
                $"Device type '{deviceType.TypeName}' created.",
                requestingUserId);

            return (await GetByIdAsync(deviceType.Id))!;
        }
    }
}
