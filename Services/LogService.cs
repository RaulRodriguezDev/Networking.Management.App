using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;
using Network.Management.App.Repository.Models;

namespace Network.Management.App.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;

        private static readonly string[] AuthActions =
        [
            LogActionType.UserLogin,
            LogActionType.UserLoginFailed,
            LogActionType.UserLogout
        ];

        // Kept as an expression tree (not a method call) so EF Core can translate the projection,
        // including the User/Device navigations, into a single SQL query with the required JOINs.
        private static readonly Expression<Func<LogWithCategory, LogDto>> ProjectExpression = x => new LogDto
        {
            LogId = x.Log.LogId,
            ActionType = x.Log.ActionType,
            Details = x.Log.Details,
            CreatedAt = x.Log.CreatedAt,
            Category = x.Category,
            UserId = x.Log.UserId,
            Username = x.Log.User.Username,
            DeviceId = x.Log.DeviceId,
            DeviceName = x.Log.Device != null ? x.Log.Device.DeviceName : null
        };

        public LogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LogDto>> GetLogsAsync(IEnumerable<string> allowedCategories, string? category = null)
        {
            var allowed = allowedCategories.ToHashSet();

            // A requested category is only honored when it is part of what the user is allowed to see.
            if (!string.IsNullOrWhiteSpace(category) && allowed.Contains(category))
                allowed = [category];

            var logs = await QueryWithCategory()
                .Where(x => allowed.Contains(x.Category))
                .OrderByDescending(x => x.Log.CreatedAt)
                .Select(ProjectExpression)
                .ToListAsync();

            return logs;
        }

        public async Task<IEnumerable<LogDto>> GetLogsByUserAsync(int userId)
        {
            return await QueryWithCategory()
                .Where(x => x.Log.UserId == userId)
                .OrderByDescending(x => x.Log.CreatedAt)
                .Select(ProjectExpression)
                .ToListAsync();
        }

        private IQueryable<LogWithCategory> QueryWithCategory()
        {
            return _context.Logs.Select(l => new LogWithCategory
            {
                Log = l,
                Category = AuthActions.Contains(l.ActionType) ? LogCategory.Auth
                           : l.DeviceId != null ? LogCategory.Device
                           : LogCategory.App
            });
        }

        private sealed class LogWithCategory
        {
            public required Log Log { get; set; }
            public required string Category { get; set; }
        }
    }
}
