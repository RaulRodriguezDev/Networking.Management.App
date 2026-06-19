using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceTypesController : ControllerBase
    {
        private readonly IDeviceTypeService _deviceTypeService;

        public DeviceTypesController(IDeviceTypeService deviceTypeService)
        {
            _deviceTypeService = deviceTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _deviceTypeService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var deviceType = await _deviceTypeService.GetByIdAsync(id);
            return deviceType is null ? NotFound() : Ok(deviceType);
        }

        [HttpPost]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Create([FromBody] CreateDeviceTypeDto dto)
        {
            if (await _deviceTypeService.TypeNameExistsAsync(dto.TypeName))
                return Conflict(new { message = $"Device type '{dto.TypeName}' already exists." });

            var created = await _deviceTypeService.CreateAsync(dto, RequestingUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
