using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Dashboard.Dto;

namespace Sirius.Reports
{
    public interface IReportAppService : IApplicationService
    {
        Task<DashboardDto> GetDashboardData();

        Task<FinancialStatementDto> GetFinancialStatement();
    }
}