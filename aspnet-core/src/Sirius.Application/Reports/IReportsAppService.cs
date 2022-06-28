using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Reports.Dto;

namespace Sirius.Reports
{
    public interface IReportsAppService : IApplicationService
    {
        Task<List<HousingDueReportDto>> GetHousingDueReport(HousingDueReportFilter filter);
    }
}