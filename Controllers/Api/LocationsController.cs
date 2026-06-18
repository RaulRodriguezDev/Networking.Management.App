using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _locationService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            return location is null ? NotFound() : Ok(location);
        }

        [HttpPost]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            if (await _locationService.LocationNameExistsAsync(dto.LocationName))
                return Conflict(new { message = $"Location '{dto.LocationName}' already exists." });

            var created = await _locationService.CreateAsync(dto, RequestingUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLocationDto dto)
        {
            if (await _locationService.LocationNameExistsAsync(dto.LocationName, id))
                return Conflict(new { message = $"Location '{dto.LocationName}' already exists." });

            var updated = await _locationService.UpdateAsync(id, dto, RequestingUserId());
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppRoles.AdminOrIT)]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _locationService.HasDevicesAsync(id))
                return Conflict(new { message = "Cannot delete a location that still has devices assigned." });

            var deleted = await _locationService.DeleteAsync(id, RequestingUserId());
            return deleted ? NoContent() : NotFound();
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}

