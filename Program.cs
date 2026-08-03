using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Network.Management.App.Middleware;
using Network.Management.App.Repository;
using Network.Management.App.Services;

var builder = WebApplication.CreateBuilder(args);
var cookieExpiry = builder.Configuration.GetValue<int>("Auth:CookieExpiryMinutes");

// When false (e.g. for a quick LAN test over plain HTTP) cookie security and HTTPS
// redirection are relaxed. Defaults to true so production stays secure unless explicitly opted out.
var requireHttps = builder.Configuration.GetValue("Auth:RequireHttps", true);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbRepository, DbRepository>();
builder.Services.AddScoped<IAppLogger, AppLogger>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceTypeService, DeviceTypeService>();
builder.Services.AddScoped<INetworkSettingService, NetworkSettingService>();
builder.Services.AddScoped<IVlanService, VlanService>();
builder.Services.AddScoped<IDeviceOperationsService, DeviceOperationsService>();
builder.Services.AddScoped<IDeviceDiscoveryService, DeviceDiscoveryService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();

// Reachability (real ICMP ping) + the background monitor that keeps device status in sync.
builder.Services.AddSingleton<IReachabilityService, ReachabilityService>();
builder.Services.AddSingleton<IMacAddressResolver, MacAddressResolver>();
builder.Services.AddHostedService<DeviceMonitorService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Account/Unauthorized";
    options.AccessDeniedPath = "/Account/Unauthorized";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieExpiry);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = requireHttps ? SameSiteMode.Strict : SameSiteMode.Lax;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.AdminOnly, p => p.RequireRole(AppRoles.Admin));
    options.AddPolicy(AppRoles.AdminOrIT, p => p.RequireRole(AppRoles.Admin, AppRoles.IT));
});

builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css", "css/layout.css", "css/views/*.css");
    pipeline.AddJavaScriptBundle("/js/bundle.js", "js/layout.js", "js/views/*.js");
});

var app = builder.Build();

// Always seed essential infrastructure (roles + the reserved 'system' account for automated logging).
// Seed the test users only in Development, or when explicitly enabled (e.g. a published LAN test).
var seederLogger = app.Services.GetRequiredService<ILogger<Program>>();
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue("SeedData", false))
{
    await DataSeeder.SeedAsync(app.Services, seederLogger);
}
else
{
    await DataSeeder.SeedEssentialsAsync(app.Services, seederLogger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (requireHttps)
{
    app.UseHttpsRedirection();
}

// Restrict access by client IP (config allowlist + admin-editable DB setting). Runs before
// static files so unauthorized computers cannot reach any part of the app.
app.UseIpAccessRestriction();

app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
