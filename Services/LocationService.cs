using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public LocationService(AppDbContext context, IAppLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<LocationDto>> GetAllAsync()
        {
            return await _context.DeviceLocations
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    LocationName = l.LocationName,
                    LocationFloor = l.LocationFloor,
                    TotalDevices = l.Devices.Count,
                    OnlineDevices = l.Devices.Count(d => d.Status == "Online"),
                    OfflineDevices = l.Devices.Count(d => d.Status == "Offline"),
                    Status = l.Devices.Count == 0 ? "unknown"
                             : l.Devices.Any(d => d.Status == "Offline") ? "offline"
                             : "online"
                })
                .ToListAsync();
        }

        public async Task<LocationDto?> GetByIdAsync(int locationId)
        {
            return await _context.DeviceLocations
                .Where(l => l.Id == locationId)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    LocationName = l.LocationName,
                    LocationFloor = l.LocationFloor,
                    TotalDevices = l.Devices.Count,
                    OnlineDevices = l.Devices.Count(d => d.Status == "Online"),
                    OfflineDevices = l.Devices.Count(d => d.Status == "Offline"),
                    Status = l.Devices.Count == 0 ? "unknown"
                             : l.Devices.Any(d => d.Status == "Offline") ? "offline"
                             : "online"
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> LocationNameExistsAsync(string locationName, int? excludeId = null)
        {
            var normalized = locationName.Trim();
            return await _context.DeviceLocations
                .AnyAsync(l => l.LocationName == normalized && (excludeId == null || l.Id != excludeId));
        }

        public async Task<bool> HasDevicesAsync(int locationId)
        {
            return await _context.Devices.AnyAsync(d => d.DeviceLocationId == locationId);
        }

        public async Task<LocationDto> CreateAsync(CreateLocationDto dto, int requestingUserId)
        {
            var location = new DeviceLocation
            {
                LocationName = dto.LocationName.Trim(),
                LocationFloor = dto.LocationFloor.Trim()
            };

            _context.DeviceLocations.Add(location);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.LocationCreated, $"Location '{location.LocationName}' created.", requestingUserId);

            return (await GetByIdAsync(location.Id))!;
        }

        public async Task<LocationDto?> UpdateAsync(int locationId, UpdateLocationDto dto, int requestingUserId)
        {
            var location = await _context.DeviceLocations.FindAsync(locationId);
            if (location is null) return null;

            location.LocationName = dto.LocationName.Trim();
            location.LocationFloor = dto.LocationFloor.Trim();

            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.LocationUpdated, $"Location '{location.LocationName}' updated.", requestingUserId);

            return await GetByIdAsync(locationId);
        }

        public async Task<bool> DeleteAsync(int locationId, int requestingUserId)
        {
            var location = await _context.DeviceLocations.FindAsync(locationId);
            if (location is null) return false;

            _context.DeviceLocations.Remove(location);
            await _context.SaveChangesAsync();

            await _logger.PersistAsync(LogActionType.LocationDeleted, $"Location '{location.LocationName}' deleted.", requestingUserId);

            return true;
        }
    }
}
