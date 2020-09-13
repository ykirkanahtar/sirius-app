using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.HousingCategories.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.HousingCategories
{
    public interface IHousingCategoryAppService : IAsyncCrudAppService<HousingCategoryDto, Guid, PagedHousingCategoryResultRequestDto, CreateHousingCategoryDto, UpdateHousingCategoryDto>
    {
        Task<List<LookUpDto>> GetHousingCategoryLookUpAsync();
    }
}
