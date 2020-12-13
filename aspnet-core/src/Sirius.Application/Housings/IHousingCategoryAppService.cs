using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Housings.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public interface IHousingCategoryAppService : IAsyncCrudAppService<HousingCategoryDto, Guid, PagedHousingCategoryResultRequestDto, CreateHousingCategoryDto, UpdateHousingCategoryDto>
    {
        Task<List<LookUpDto>> GetHousingCategoryLookUpAsync();
        Task<List<string>> GetHousingCategoryFromAutoCompleteFilterAsync(string request);
    }
}
