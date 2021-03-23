﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.Housings.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public interface IHousingAppService : IAsyncCrudAppService<HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>
    {
        Task<UpdateHousingDto> GetHousingForUpdate(Guid id);
        Task<List<LookUpDto>> GetHousingLookUpAsync();
        Task AddPersonAsync(CreateHousingPersonDto input);
        Task RemovePersonAsync(RemoveHousingPersonDto input);
        Task<List<LookUpDto>> GetPeopleLookUpAsync(Guid housingId);
        Task<PagedResultDto<HousingPersonDto>> GetHousingPeopleAsync(PagedHousingPersonResultRequestDto input);
        Task<List<LookUpDto>> GetHousingsLookUpByPersonIdAsync(Guid personId);
        Task<PagedResultDto<HousingForListDto>> GetAllListAsync(PagedHousingResultRequestDto input);
    }
}
