using Microsoft.EntityFrameworkCore;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public static class DataSeeder
    {
        /// <summary>Reserved account that automated processes (e.g. the device monitor) attribute their logs to.</summary>
        public const string SystemUsername = "system";

        private record SeedUser(string Name, string Username, string Password, string RoleName);

        private static readonly SeedUser[] TestUsers =
        [
            new("Administrator", "admin",   "Admin@123!", AppRoles.Admin),
            new("IT User",       "it_user", "IT@1234!",   AppRoles.IT),
            new("Guest User",    "guest",   "Guest@123!", AppRoles.Guest)
        ];

        public static async Task SeedAsync(IServiceProvider services, ILogger logger)
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(context, logger);
            await SeedUsersAsync(context, logger);
            await SeedSystemUserAsync(context, logger);
        }

        /// <summary>
        /// Seeds only the infrastructure the app needs to run correctly in any environment: the roles
        /// and the reserved 'system' account used for automated logging. Safe to always call.
        /// </summary>
        public static async Task SeedEssentialsAsync(IServiceProvider services, ILogger logger)
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(context, logger);
            await SeedSystemUserAsync(context, logger);
        }

        private static async Task SeedRolesAsync(AppDbContext context, ILogger logger)
        {
            string[] roleNames = [AppRoles.Admin, AppRoles.IT, AppRoles.Guest];

            foreach (var name in roleNames)
            {
                if (!await context.Roles.AnyAsync(r => r.RoleName == name))
                {
                    context.Roles.Add(new Role
                    {
                        RoleName = name,
                        Description = name switch
                        {
                            AppRoles.Admin => "Full system access.",
                            AppRoles.IT    => "IT staff — device and location management.",
                            _              => "Read-only access to network data."
                        }
                    });

                    logger.LogInformation("[DataSeeder] Role '{Role}' seeded.", name);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(AppDbContext context, ILogger logger)
        {
            foreach (var seed in TestUsers)
            {
                if (await context.Users.AnyAsync(u => u.Username == seed.Username))
                    continue;

                var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == seed.RoleName);

                if (role is null)
                {
                    logger.LogWarning("[DataSeeder] Role '{Role}' not found, skipping user '{User}'.", seed.RoleName, seed.Username);
                    continue;
                }

                context.Users.Add(new User
                {
                    Name     = seed.Name,
                    Username = seed.Username,
                    Password = PasswordHasher.Hash(seed.Password),
                    RoleId   = role.RoleId
                });

                logger.LogInformation("[DataSeeder] User '{Username}' seeded with role '{Role}'.", seed.Username, seed.RoleName);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds the reserved 'system' account used to attribute automated log entries. It is given
        /// a non-verifiable password so it can never be logged into, and is hidden from the Users UI.
        /// </summary>
        private static async Task SeedSystemUserAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Users.AnyAsync(u => u.Username == SystemUsername))
                return;

            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == AppRoles.Admin);
            if (role is null)
            {
                logger.LogWarning("[DataSeeder] Admin role not found, skipping '{User}' account.", SystemUsername);
                return;
            }

            context.Users.Add(new User
            {
                Name = "System",
                Username = SystemUsername,
                // Sentinel value that PasswordHasher.Verify can never match (no salt.hash format) -> login disabled.
                Password = "!",
                RoleId = role.RoleId
            });

            await context.SaveChangesAsync();
            logger.LogInformation("[DataSeeder] Reserved '{Username}' account seeded.", SystemUsername);
        }
    }
}
