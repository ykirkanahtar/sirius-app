using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Periods.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Periods
{
    public interface IPeriodAppService : IAsyncCrudAppService<PeriodDto, Guid, PagedPeriodResultRequestDto,
        CreatePeriodForSiteDto, UpdatePeriodDto>
    {
        Task<PeriodDto> CreateForSiteAsync(CreatePeriodForSiteDto input);
        Task<PeriodDto> CreateForBlockAsync(CreatePeriodForBlockDto input);
        Task<List<LookUpDto>> GetPeriodLookUpAsync();
    }
}