using Microsoft.EntityFrameworkCore;
using Network.Management.App.Models.DTOs;
using Network.Management.App.Repository;

namespace Network.Management.App.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IUnknownDeviceTracker _unknownTracker;

        // Actions treated as authentication activity, mirroring how LogService categorizes logs.
        private static readonly string[] AuthActions =
        [
            LogActionType.UserLogin,
            LogActionType.UserLoginFailed,
            LogActionType.UserLogout
        ];

        public ReportService(AppDbContext context, IUnknownDeviceTracker unknownTracker)
        {
            _context = context;
            _unknownTracker = unknownTracker;
        }

        public async Task<ReportOverviewDto> GetOverviewAsync()
        {
            var total = await _context.Devices.CountAsync();
            var online = await _context.Devices.CountAsync(d => d.Status == DeviceStatus.Online);
            var offline = total - online;

            var summary = new DeviceSummaryDto
            {
                Total = total,
                Online = online,
                Offline = offline,
                Unregistered = _unknownTracker.Snapshot().Count,
                AvailabilityPercent = total == 0 ? 0 : Math.Round(online * 100.0 / total, 1)
            };

            var byType = await _context.Devices
                .GroupBy(d => d.DeviceType.TypeName)
                .Select(g => new CountItemDto { Label = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            var byLocation = await _context.Devices
                .GroupBy(d => d.DeviceLocation.LocationName)
                .Select(g => new CountItemDto { Label = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            var byVlan = await _context.Devices
                .GroupBy(d => d.Vlan != null ? d.Vlan.Name : "Unassigned")
                .Select(g => new CountItemDto { Label = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToListAsync();

            var authCount = await _context.Logs.CountAsync(l => AuthActions.Contains(l.ActionType));
            var deviceCount = await _context.Logs.CountAsync(l => l.DeviceId != null && !AuthActions.Contains(l.ActionType));
            var totalLogs = await _context.Logs.CountAsync();
            var appCount = totalLogs - authCount - deviceCount;

            var activityByCategory = new List<CountItemDto>
            {
                new() { Label = "Authentication", Count = authCount },
                new() { Label = "Devices", Count = deviceCount },
                new() { Label = "Application", Count = appCount }
            };

            var topActions = await _context.Logs
                .GroupBy(l => l.ActionType)
                .Select(g => new CountItemDto { Label = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .Take(6)
                .ToListAsync();

            var since = DateTime.UtcNow.AddDays(-7);
            var recentStatusCount = await _context.Logs
                .CountAsync(l => l.ActionType == LogActionType.DeviceStatusChanged && l.CreatedAt >= since);

            var recentStatusChanges = await _context.Logs
                .Where(l => l.ActionType == LogActionType.DeviceStatusChanged)
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .Select(l => new StatusChangeLogDto
                {
                    CreatedAt = l.CreatedAt,
                    DeviceName = l.Device != null ? l.Device.DeviceName : null,
                    Details = l.Details
                })
                .ToListAsync();

            return new ReportOverviewDto
            {
                DeviceSummary = summary,
                DevicesByType = byType,
                DevicesByLocation = byLocation,
                DevicesByVlan = byVlan,
                ActivityByCategory = activityByCategory,
                TopActions = topActions,
                RecentStatusChangeCount = recentStatusCount,
                RecentStatusChanges = recentStatusChanges,
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
    }
}
