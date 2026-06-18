using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Network.Management.App.Services;
using System.Security.Claims;

namespace Network.Management.App.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAppLogger _logger;

        public AccountController(IUserService userService, IAppLogger logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Unauthorized(string? returnUrl = null)
        {
            if (User.Identity is { IsAuthenticated: true })
                ViewData["AccessDenied"] = true;

            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Shared/Unauthorized.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = await _userService.ValidateAsync(username, password);

            if (user is not null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Name, user.Name),
                    new(ClaimTypes.Role, user.Role.RoleName),
                    new("Username", user.Username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                await _logger.PersistAsync(LogActionType.UserLogin, $"User '{user.Username}' logged in.", user.UserId);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            _logger.Warning(LogActionType.UserLoginFailed, $"Failed login attempt for username '{username}'.");
            TempData["LoginError"] = "Invalid username or password.";
            return RedirectToAction(nameof(Unauthorized), new { returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value is string idStr
                && int.TryParse(idStr, out var userId))
            {
                await _logger.PersistAsync(LogActionType.UserLogout, $"User '{User.FindFirst("Username")?.Value}' logged out.", userId);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}

