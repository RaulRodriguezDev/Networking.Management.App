using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
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
        private readonly IUnknownDeviceTracker _unknownTracker;
        private readonly IStatusChangeTracker _statusChanges;

        public DeviceDiscoveryController(
            IDeviceDiscoveryService discoveryService,
            IUnknownDeviceTracker unknownTracker,
            IStatusChangeTracker statusChanges)
        {
            _discoveryService = discoveryService;
            _unknownTracker = unknownTracker;
            _statusChanges = statusChanges;
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

        [HttpGet("unknown")]
        public IActionResult Unknown()
        {
            return Ok(_unknownTracker.Snapshot());
        }

        [HttpPost("unknown/dismiss")]
        public IActionResult DismissUnknown([FromBody] DismissUnknownDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.IpAddress))
                return BadRequest(new { message = "An IP address is required." });

            _unknownTracker.Dismiss(dto.IpAddress);
            return Ok(_unknownTracker.Snapshot());
        }

        [HttpPost("unknown/dismiss-all")]
        public IActionResult DismissAllUnknown()
        {
            _unknownTracker.DismissAll();
            return Ok(_unknownTracker.Snapshot());
        }

        [HttpGet("status-changes")]
        public IActionResult StatusChanges()
        {
            return Ok(_statusChanges.Snapshot());
        }

        [HttpPost("status-changes/dismiss")]
        public IActionResult DismissStatusChange([FromBody] DismissStatusChangeDto dto)
        {
            if (dto is null || dto.Id <= 0)
                return BadRequest(new { message = "A valid notification id is required." });

            _statusChanges.Dismiss(dto.Id);
            return Ok(_statusChanges.Snapshot());
        }

        [HttpPost("status-changes/dismiss-all")]
        public IActionResult DismissAllStatusChanges()
        {
            _statusChanges.DismissAll();
            return Ok(_statusChanges.Snapshot());
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
