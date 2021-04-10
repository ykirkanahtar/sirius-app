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
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;

namespace Sirius.People
{
    public class PersonAppService :
        AsyncCrudAppService<Person, PersonDto, Guid, PagedPersonResultRequestDto, CreatePersonDto, UpdatePersonDto>,
        IPersonAppService
    {
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;

        public PersonAppService(IPersonManager personManager, IRepository<Person, Guid> personRepository,
            IRepository<HousingPerson, Guid> housingPersonRepository, IRepository<Housing, Guid> housingRepository,
            IPaymentCategoryManager paymentCategoryManager)
            : base(personRepository)
        {
            _personManager = personManager;
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
            _housingRepository = housingRepository;
            _paymentCategoryManager = paymentCategoryManager;
        }

        public override async Task<PersonDto> CreateAsync(CreatePersonDto input)
        {
            CheckCreatePermission();
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
            CheckUpdatePermission();
            var existingPerson = await _personManager.GetAsync(input.Id);
            var person = Person.Update(existingPerson, input.FirstName, input.LastName, input.Phone1, input.Phone2);
            await _personManager.UpdateAsync(person);
            return ObjectMapper.Map<PersonDto>(person);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var person = await _personManager.GetAsync(input.Id);
            await _personManager.DeleteAsync(person);
        }

        public override async Task<PagedResultDto<PersonDto>> GetAllAsync(PagedPersonResultRequestDto input)
        {
            CheckGetAllPermission();

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
                         p.person.Phone2.Contains(input.PhoneNumber))
                .GroupBy(p => new PersonDto
                {
                    Id = p.person.Id,
                    FirstName = p.person.FirstName,
                    LastName = p.person.LastName,
                    Phone1 = p.person.Phone1,
                    Phone2 = p.person.Phone2
                }).Select(p => p.Key);

            var people = await query
                .OrderBy(input.Sorting ?? $"{nameof(PersonDto.FirstName)} ASC, {nameof(PersonDto.LastName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PersonDto>(query.Count(),
                ObjectMapper.Map<List<PersonDto>>(people));
        }
        
        public async Task<List<LookUpDto>> GetPersonLookUpAsync()
        {
            CheckGetAllPermission();

            var people = await _personRepository.GetAll().ToListAsync();

            return
                (from l in people
                    orderby l.FirstName, l.LastName
                    select new LookUpDto(l.Id.ToString(), $"{l.FirstName} {l.LastName}")).ToList();
        }

        public async Task<List<LookUpDto>> GetPersonLookUpForHousingDueAsync(PersonLookUpFilter filter)
        {
            CheckGetAllPermission();

            var query = _personRepository.GetAll();

            if (filter.PaymentCategoryId.HasValue)
            {
                var paymentCategory = await _paymentCategoryManager.GetAsync(filter.PaymentCategoryId.Value);
                var housingCategoryIds = await _paymentCategoryManager.GetHousingCategories(paymentCategory.Id);

                var personIds = await (from h in _housingRepository.GetAll()
                    join housingPerson in _housingPersonRepository.GetAll() on h.Id equals housingPerson.HousingId
                    where housingCategoryIds.Contains(h.HousingCategoryId)
                    select housingPerson.PersonId).ToListAsync();

                query = query.Where(p => personIds.Distinct().Contains(p.Id));
            }

            var people = await query.ToListAsync();

            return
                (from l in people
                    orderby l.FirstName, l.LastName
                    select new LookUpDto(l.Id.ToString(), $"{l.FirstName} {l.LastName}")).ToList();
        }

        public async Task<List<string>> GetPeopleFromAutoCompleteFilterAsync(string request)
        {
            CheckGetAllPermission();

            var query = from p in _personRepository.GetAll()
                where p.FirstName.Contains(request) || p.LastName.Contains(request)
                select p.Name;

            return await query.ToListAsync();
        }
    }
}