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
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingCategories;
using Sirius.Housings.Dto;
using Sirius.People;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public class HousingAppService : AsyncCrudAppService<Housing, HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto>, IHousingAppService
    {
        private readonly IHousingManager _housingManager;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPerson> _housingPersonRepository;

        public HousingAppService(IHousingManager housingManager, IRepository<Housing, Guid> housingRepository, IRepository<HousingCategory, Guid> housingCategoryRepository, IPersonManager personManager, IRepository<Person, Guid> personRepository, IRepository<HousingPerson> housingPersonRepository)
            : base(housingRepository)
        {
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _personManager = personManager;
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);

            var housing = Housing.Create(SequentialGuidGenerator.Instance.Create(), AbpSession.GetTenantId(), input.Block, input.Apartment, housingCategory);
            await _housingManager.CreateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task<HousingDto> UpdateAsync(UpdateHousingDto input)
        {
            var existingHousing = await _housingManager.GetAsync(input.Id);
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);

            var housing = Housing.Update(existingHousing, input.Block, input.Apartment, housingCategory);
            await _housingManager.UpdateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }

        public override async Task<PagedResultDto<HousingDto>> GetAllAsync(PagedHousingResultRequestDto input)
        {
            var query = _housingRepository.GetAll()
                .Include(p => p.HousingCategory);

            var housings = await query.ToListAsync();

            return new PagedResultDto<HousingDto>(housings.Count,
                ObjectMapper.Map<List<HousingDto>>(housings));
        }

        public async Task<List<LookUpDto>> GetHousingLookUpAsync()
        {
            var housings = await _housingRepository.GetAllListAsync();

            return
                (from l in housings
                 select new LookUpDto(l.Id.ToString(), l.GetName())).ToList();
        }

        public async Task<HousingPersonDto> AddPersonAsync(CreateHousingPersonDto input)
        {
            var housing = await _housingManager.GetAsync(input.HousingId);
            var person = await _personManager.GetAsync(input.PersonId);

            var housingPerson = await _housingManager.AddPersonAsync(housing, person, input.HousingPersonType, input.Contact);
            return ObjectMapper.Map<HousingPersonDto>(housingPerson);
        }

        public async Task<List<LookUpDto>> GetPeopleLookUpAsync(Guid housingId)
        {
            var housing = await _housingRepository.GetAsync(housingId);

            var housingPeople = await _housingPersonRepository.GetAllListAsync(p => p.HousingId == housingId);

            var peopleQuery = _personRepository.GetAll();
            var people = await peopleQuery.WhereIf(housingPeople.Count > 0, p => housingPeople.Select(x => x.PersonId).Contains(p.Id) == false).ToListAsync();


            if (people.Count == 0)
            {
                throw new UserFriendlyException("Sistemde uygun kişi bulunamadı.");
            }

            return people.Select(p => new LookUpDto(p.Id.ToString(), $"{p.FirstName} {p.LastName}")).ToList();
        }
    }
}
