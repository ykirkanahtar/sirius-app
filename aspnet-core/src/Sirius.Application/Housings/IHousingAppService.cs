using System;
using Abp.Application.Services;
using Sirius.Housings.Dto;

namespace Sirius.Housings
{
    public interface IHousingAppService : IAsyncCrudAppService<HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>
    {

    }
}
