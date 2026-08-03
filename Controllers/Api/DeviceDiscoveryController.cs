using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [ApiController]
    [Route("api/devices")]
    [Authorize(Policy = AppRoles.AdminOrIT)]
    public class DeviceDiscoveryController : ControllerBase
    {
        private readonly IDeviceDiscoveryService _discoveryService;

        public DeviceDiscoveryController(IDeviceDiscoveryService discoveryService)
        {
            _discoveryService = discoveryService;
        }

        [HttpPost("discover")]
        public async Task<IActionResult> Discover()
        {
            try
            {
                return Ok(await _discoveryService.DiscoverAsync(RequestingUserId()));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
