using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDbRepository _repository;

        public DevicesController(IDbRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("by-location/{locationId:int}")]
        public async Task<IActionResult> GetByLocation(int locationId)
        {
            var devices = await _repository.GetWhereAsync<Device, DeviceDto>(
                d => d.DeviceLocationId == locationId,
                d => new DeviceDto
                {
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    IpAddress = d.IpAddress,
                    Status = d.Status,
                    DeviceType = d.DeviceType.TypeName
                });

            return Ok(devices);
        }
    }
}
