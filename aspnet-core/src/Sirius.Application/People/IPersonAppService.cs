using System;
using Abp.Application.Services;
using Sirius.People.Dto;

namespace Sirius.People
{
    public interface IPersonAppService : IAsyncCrudAppService<PersonDto, Guid, PagedPersonResultRequestDto, CreatePersonDto, UpdatePersonDto>
    {

    }
}
