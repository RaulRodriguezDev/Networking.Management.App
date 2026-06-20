using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOrIT)]
    [ApiController]
    [Route("api/[controller]")]
    public class VlansController : ControllerBase
    {
        private readonly IVlanService _vlanService;

        public VlansController(IVlanService vlanService)
        {
            _vlanService = vlanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _vlanService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vlan = await _vlanService.GetByIdAsync(id);
            return vlan is null ? NotFound() : Ok(vlan);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveVlanDto dto)
        {
            if (await _vlanService.VlanNumberExistsAsync(dto.VlanNumber))
                return Conflict(new { message = $"VLAN {dto.VlanNumber} already exists." });

            var created = await _vlanService.CreateAsync(dto, RequestingUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.VlanId }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveVlanDto dto)
        {
            if (await _vlanService.VlanNumberExistsAsync(dto.VlanNumber, id))
                return Conflict(new { message = $"VLAN {dto.VlanNumber} already exists." });

            var updated = await _vlanService.UpdateAsync(id, dto, RequestingUserId());
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = AppRoles.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _vlanService.DeleteAsync(id, RequestingUserId());
            return deleted ? NoContent() : NotFound();
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
