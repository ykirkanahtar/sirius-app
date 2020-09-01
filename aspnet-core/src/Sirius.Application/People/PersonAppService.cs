using System;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.People.Dto;

namespace Sirius.People
{
    public class PersonAppService : AsyncCrudAppService<Person, PersonDto, Guid, PagedPersonResultRequestDto, CreatePersonDto, UpdatePersonDto>, IPersonAppService
    {
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;

        public PersonAppService(IPersonManager personManager, IRepository<Person, Guid> personRepository)
        : base(personRepository)
        {
            _personManager = personManager;
            _personRepository = personRepository;
        }

        public override async Task<PersonDto> CreateAsync(CreatePersonDto input)
        {
            var person = Person.Create(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.FirstName
                    , input.LastName
                    , input.Phone1
                    , input.Phone2
                    );
            await _personManager.CreateAsync(person);
            return ObjectMapper.Map<PersonDto>(person);
        }

        public override async Task<PersonDto> UpdateAsync(UpdatePersonDto input)
        {
            var existingPerson = await _personManager.GetAsync(input.Id);
            var person = Person.Update(existingPerson, input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _personManager.UpdateAsync(person);
            return ObjectMapper.Map<PersonDto>(person);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var person = await _personManager.GetAsync(input.Id);
            await _personManager.DeleteAsync(person);
        }
    }
}
