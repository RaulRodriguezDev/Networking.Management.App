using Network.Management.App.Models.DTOs;

namespace Network.Management.App.Services
{
    public interface IReportService
    {
        Task<ReportOverviewDto> GetOverviewAsync();
    }
}
