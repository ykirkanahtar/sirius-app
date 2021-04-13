using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Dashboard.Dto;

namespace Sirius.Dashboard
{
    public interface IDashboardAppService : IApplicationService
    {
        Task<DashboardDto> GetDashboardData();
    }
}