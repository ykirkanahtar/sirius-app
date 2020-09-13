using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.Housings.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public interface IHousingAppService : IAsyncCrudAppService<HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>
    {
        Task<List<LookUpDto>> GetHousingLookUpAsync();
    }
}
