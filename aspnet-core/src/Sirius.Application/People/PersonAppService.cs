using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.Housings;
using Sirius.People.Dto;
using Sirius.Shared.Dtos;
using System.Linq.Dynamic.Core;

namespace Sirius.People
{
    public class PersonAppService :
        AsyncCrudAppService<Person, PersonDto, Guid, PagedPersonResultRequestDto, CreatePersonDto, UpdatePersonDto>,
        IPersonAppService
    {
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPerson> _housingPersonRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;

        public PersonAppService(IPersonManager personManager, IRepository<Person, Guid> personRepository,
            IRepository<HousingPerson> housingPersonRepository, IRepository<Housing, Guid> housingRepository)
            : base(personRepository)
        {
            _personManager = personManager;
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
            _housingRepository = housingRepository;
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

        public override async Task<PagedResultDto<PersonDto>> GetAllAsync(PagedPersonResultRequestDto input)
        {
            var query = (from person in _personRepository.GetAll()
                    join housingPerson in _housingPersonRepository.GetAll() on person.Id equals housingPerson
                            .PersonId
                        into g1
                    from housingPerson in g1.DefaultIfEmpty()
                    join housing in _housingRepository.GetAll() on housingPerson.HousingId equals housing.Id into g2
                    from housing in g2.DefaultIfEmpty()
                    select new {person, housingPerson, housing})
                .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                    p => (p.person.FirstName + " " + p.person.LastName).Contains(input.Name))
                .WhereIf(input.HousingIds.Count > 0, p => input.HousingIds.Contains(p.housing.Id))
                .WhereIf(!string.IsNullOrWhiteSpace(input.PhoneNumber),
                    p => p.person.Phone1.Contains(input.PhoneNumber) ||
                         p.person.Phone2.Contains(input.PhoneNumber));

            var people = await query.Select(p => p.person).OrderBy(input.Sorting ?? $"{nameof(PersonDto.FirstName)} ASC, {nameof(PersonDto.LastName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PersonDto>(query.Count(),
                ObjectMapper.Map<List<PersonDto>>(people));
        }
        
        public async Task<List<LookUpDto>> GetPersonLookUpAsync()
        {
            var people = await _personRepository.GetAllListAsync();

            return
                (from l in people
                    select new LookUpDto(l.Id.ToString(), $"{l.FirstName} {l.LastName}")).ToList();
        }

        public async Task<List<string>> GetPeopleFromAutoCompleteFilterAsync(string request)
        {
            var query = from p in _personRepository.GetAll()
                where p.FirstName.Contains(request) || p.LastName.Contains(request)
                select p.Name;

            return await query.ToListAsync();
        }
    }
}