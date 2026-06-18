using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Network.Management.App.Repository;
using Network.Management.App.Services;

var builder = WebApplication.CreateBuilder(args);
var cookieExpiry = builder.Configuration.GetValue<int>("Auth:CookieExpiryMinutes");

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
builder.Services.AddScoped<ILogService, LogService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Unauthorized";
        options.AccessDeniedPath = "/Account/Unauthorized";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieExpiry);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
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

// Seed roles and test users in Development only.
if (app.Environment.IsDevelopment())
{
    var seederLogger = app.Services.GetRequiredService<ILogger<Program>>();
    await DataSeeder.SeedAsync(app.Services, seederLogger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
