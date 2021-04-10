using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.People.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.People
{
    public interface IPersonAppService : IAsyncCrudAppService<PersonDto, Guid, PagedPersonResultRequestDto, CreatePersonDto, UpdatePersonDto>
    {
        Task<List<LookUpDto>> GetPersonLookUpAsync();
        Task<List<LookUpDto>> GetPersonLookUpForHousingDueAsync(PersonLookUpFilter filter);
        Task<List<string>> GetPeopleFromAutoCompleteFilterAsync(string request);
    }
}
