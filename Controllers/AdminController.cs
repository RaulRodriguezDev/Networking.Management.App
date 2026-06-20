using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Services;

namespace Network.Management.App.Controllers
{
    [Authorize(Policy = AppRoles.AdminOrIT)]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = AppRoles.AdminOnly)]
        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Locations()
        {
            return View();
        }

        public IActionResult Devices()
        {
            return View();
        }

        public IActionResult Device(int id)
        {
            ViewData["DeviceId"] = id;
            return View();
        }


        public IActionResult Logs()
        {
            return View();
        }
    }
}

