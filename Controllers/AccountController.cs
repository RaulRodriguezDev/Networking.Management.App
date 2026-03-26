using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Network.Management.App.Controllers
{
    public class AccountController : Controller
    {
        private static readonly Dictionary<string, (string Password, string Role, string DisplayName)> MockUsers = new()
        {
            ["guest"] = ("guest", "Guest", "Guest User"),
            ["admin"] = ("admin", "Admin", "Administrator")
        };

        [HttpGet]
        public IActionResult Unauthorized(string? returnUrl = null)
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                ViewData["AccessDenied"] = true;
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Shared/Unauthorized.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var key = username?.ToLowerInvariant() ?? string.Empty;

            if (MockUsers.TryGetValue(key, out var user) && user.Password == password)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.DisplayName),
                    new(ClaimTypes.Role, user.Role),
                    new("Username", key)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            TempData["LoginError"] = "Invalid username or password.";
            return RedirectToAction(nameof(Unauthorized), new { returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
