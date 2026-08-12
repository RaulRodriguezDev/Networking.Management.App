using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Services;

namespace Network.Management.App.Controllers.Api
{
    [Authorize(Policy = AppRoles.AdminOrIT)]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> Overview()
        {
            return Ok(await _reportService.GetOverviewAsync());
        }
    }
}
