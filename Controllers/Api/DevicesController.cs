using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DevicesController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _deviceService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            return device is null ? NotFound() : Ok(device);
        }

        [HttpGet("by-location/{locationId:int}")]
        public async Task<IActionResult> GetByLocation(int locationId)
        {
            return Ok(await _deviceService.GetByLocationAsync(locationId));
        }

        [HttpGet("suggest-ip")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> SuggestIp()
        {
            var ip = await _deviceService.SuggestNextIpAsync();
            return ip is null
                ? Conflict(new { message = "No free IP addresses are available in the configured range." })
                : Ok(new { ipAddress = ip });
        }

        [HttpPost]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
        {
            if (!await _deviceService.DeviceTypeExistsAsync(dto.DeviceTypeId))
                return BadRequest(new { message = $"Device type '{dto.DeviceTypeId}' does not exist." });

            if (!await _deviceService.DeviceLocationExistsAsync(dto.DeviceLocationId))
                return BadRequest(new { message = $"Location '{dto.DeviceLocationId}' does not exist." });

            var ipError = await _deviceService.ValidateIpInRangeAsync(dto.IpAddress);
            if (ipError is not null)
                return BadRequest(new { message = ipError });

            if (await _deviceService.IpAddressExistsAsync(dto.IpAddress))
                return Conflict(new { message = $"A device with IP address '{dto.IpAddress}' already exists." });

            var created = await _deviceService.CreateAsync(dto, RequestingUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.DeviceId }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto dto)
        {
            if (!await _deviceService.DeviceTypeExistsAsync(dto.DeviceTypeId))
                return BadRequest(new { message = $"Device type '{dto.DeviceTypeId}' does not exist." });

            if (!await _deviceService.DeviceLocationExistsAsync(dto.DeviceLocationId))
                return BadRequest(new { message = $"Location '{dto.DeviceLocationId}' does not exist." });

            var ipError = await _deviceService.ValidateIpInRangeAsync(dto.IpAddress);
            if (ipError is not null)
                return BadRequest(new { message = ipError });

            if (await _deviceService.IpAddressExistsAsync(dto.IpAddress, id))
                return Conflict(new { message = $"A device with IP address '{dto.IpAddress}' already exists." });

            var updated = await _deviceService.UpdateAsync(id, dto, RequestingUserId());
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpPost("{id:int}/connect")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Connect(int id)
        {
            var connected = await _deviceService.ConnectAsync(id, RequestingUserId());
            return connected is null ? NotFound() : Ok(connected);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _deviceService.DeleteAsync(id, RequestingUserId());
            return deleted ? NoContent() : NotFound();
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
