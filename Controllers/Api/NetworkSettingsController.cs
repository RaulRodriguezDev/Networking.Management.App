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
    public class NetworkSettingsController : ControllerBase
    {
        private readonly INetworkSettingService _networkSettingService;

        public NetworkSettingsController(INetworkSettingService networkSettingService)
        {
            _networkSettingService = networkSettingService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _networkSettingService.GetAsync());
        }

        [HttpPut]
        [Authorize(Policy = AppRoles.AdminOnly)]
        public async Task<IActionResult> Update([FromBody] UpdateNetworkSettingDto dto)
        {
            var updated = await _networkSettingService.UpdateAsync(dto, RequestingUserId());
            return Ok(updated);
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
