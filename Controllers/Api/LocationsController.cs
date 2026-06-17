using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly IDbRepository _repository;

        public LocationsController(IDbRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var locations = await _repository.GetAllAsync<DeviceLocation, LocationDto>(l => new LocationDto
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
            });

            return Ok(locations);
        }
    }
}
