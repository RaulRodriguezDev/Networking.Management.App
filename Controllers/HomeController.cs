using Microsoft.AspNetCore.Mvc;

namespace Network.Management.App.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
