using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOnly)]
    [ApiController]
    [Route("api/[controller]")]
    public class AccessControlController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;

        public AccessControlController(IAccessControlService accessControl)
        {
            _accessControl = accessControl;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _accessControl.GetAsync());
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAccessSettingDto dto)
        {
            if (string.Equals(dto.Mode, AccessMode.Restricted, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(dto.MasterIp))
            {
                return BadRequest(new { message = "A master PC IP address is required when access is restricted." });
            }

            var updated = await _accessControl.UpdateAsync(dto, RequestingUserId());
            return Ok(updated);
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
