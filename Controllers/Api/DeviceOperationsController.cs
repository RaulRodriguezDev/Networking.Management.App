using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    // All device operations require IT or Admin. Individual destructive actions tighten this further.
    [Authorize(Policy = AppRoles.AdminOrIT)]
    [ApiController]
    [Route("api/devices/{deviceId:int}")]
    public class DeviceOperationsController : ControllerBase
    {
        private readonly IDeviceOperationsService _operations;

        public DeviceOperationsController(IDeviceOperationsService operations)
        {
            _operations = operations;
        }

        [HttpPost("probe")]
        public async Task<IActionResult> Probe(int deviceId)
        {
            var result = await _operations.ProbeAsync(deviceId, RequestingUserId());
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status(int deviceId)
        {
            var status = await _operations.GetStatusAsync(deviceId);
            return status is null ? NotFound() : Ok(status);
        }

        [HttpPut("ip")]
        public async Task<IActionResult> ChangeIp(int deviceId, [FromBody] ChangeDeviceIpDto dto)
        {
            try
            {
                var updated = await _operations.ChangeIpAsync(deviceId, dto.IpAddress, RequestingUserId());
                return updated is null ? NotFound() : Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("vlan")]
        public async Task<IActionResult> AssignVlan(int deviceId, [FromBody] AssignVlanDto dto)
        {
            var updated = await _operations.AssignVlanAsync(deviceId, dto.VlanId, RequestingUserId());
            return updated is null
                ? NotFound(new { message = "Device or VLAN not found." })
                : Ok(updated);
        }

        [HttpPut("location")]
        public async Task<IActionResult> Move(int deviceId, [FromBody] MoveDeviceDto dto)
        {
            var updated = await _operations.MoveAsync(deviceId, dto.DeviceLocationId, RequestingUserId());
            return updated is null
                ? NotFound(new { message = "Device or location not found." })
                : Ok(updated);
        }

        // ===== Ports =====
        [HttpGet("ports")]
        public async Task<IActionResult> GetPorts(int deviceId)
        {
            return Ok(await _operations.GetPortsAsync(deviceId));
        }

        [HttpPut("ports/{portId:int}")]
        public async Task<IActionResult> ConfigurePort(int deviceId, int portId, [FromBody] ConfigurePortDto dto)
        {
            var updated = await _operations.ConfigurePortAsync(deviceId, portId, dto, RequestingUserId());
            return updated is null ? NotFound() : Ok(updated);
        }

        // ===== Routing =====
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes(int deviceId)
        {
            return Ok(await _operations.GetRoutesAsync(deviceId));
        }

        [HttpPost("routes")]
        public async Task<IActionResult> AddRoute(int deviceId, [FromBody] SaveRouteDto dto)
        {
            var created = await _operations.AddRouteAsync(deviceId, dto, RequestingUserId());
            return created is null ? NotFound() : Ok(created);
        }

        [HttpDelete("routes/{routeId:int}")]
        public async Task<IActionResult> DeleteRoute(int deviceId, int routeId)
        {
            var deleted = await _operations.DeleteRouteAsync(deviceId, routeId, RequestingUserId());
            return deleted ? NoContent() : NotFound();
        }

        private int RequestingUserId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    }
}
