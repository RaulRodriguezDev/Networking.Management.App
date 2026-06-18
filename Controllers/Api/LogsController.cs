using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOrIT)]
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogsController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? category = null)
        {
            var logs = await _logService.GetLogsAsync(AllowedCategories(), category);
            return Ok(logs);
        }

        [HttpGet("by-user/{userId:int}")]
        [Authorize(Policy = AppRoles.AdminOnly)]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var logs = await _logService.GetLogsByUserAsync(userId);
            return Ok(logs);
        }

        // Admins see every category; IT users do not see auth (user-account) logs.
        private string[] AllowedCategories() =>
            User.IsInRole(AppRoles.Admin)
                ? [LogCategory.Auth, LogCategory.Device, LogCategory.App]
                : [LogCategory.Device, LogCategory.App];
    }
}
