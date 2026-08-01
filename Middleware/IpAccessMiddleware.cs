using Network.Management.App.Services;

namespace Network.Management.App.Middleware
{
    /// <summary>
    /// Restricts access to the application based on the caller's IP address. The allowlist comes
    /// from two sources: configuration (web.config / appsettings, requires an IIS restart) and the
    /// admin-editable database setting (takes effect immediately). Loopback is always allowed.
    /// </summary>
    public class IpAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpAccessMiddleware> _logger;

        public IpAccessMiddleware(RequestDelegate next, ILogger<IpAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // IAccessControlService is scoped, so it is resolved per-request via method injection.
        public async Task InvokeAsync(HttpContext context, IAccessControlService accessControl)
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            if (await accessControl.IsAllowedAsync(clientIp))
            {
                await _next(context);
                return;
            }

            _logger.LogWarning("[IpAccess] Blocked request from {ClientIp} to {Path}.", clientIp, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(
                "Access denied. This application is restricted to authorized computers only.");
        }
    }

    public static class IpAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpAccessRestriction(this IApplicationBuilder app)
            => app.UseMiddleware<IpAccessMiddleware>();
    }
}
