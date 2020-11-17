using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.Authorization;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.HousingCategories;
using Sirius.HousingPaymentPlans;
using Sirius.Housings.Dto;
using Sirius.PaymentCategories;
using Sirius.People;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    [AbpAuthorize(PermissionNames.Pages_Housings)]
    public class HousingAppService :
        AsyncCrudAppService<Housing, HousingDto, Guid, PagedHousingResultRequestDto, CreateHousingDto, UpdateHousingDto
        >, IHousingAppService
    {
        private readonly IHousingManager _housingManager;
        private readonly IHousingRepository _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IPersonManager _personManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPerson> _housingPersonRepository;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IHousingPolicy _housingPolicy;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;

        public HousingAppService(IHousingManager housingManager, IHousingRepository housingRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository, IPersonManager personManager,
            IRepository<Person, Guid> personRepository, IRepository<HousingPerson> housingPersonRepository,
            IRepository<Block, Guid> blockRepository, IHousingPolicy housingPolicy,
            IPaymentCategoryManager paymentCategoryManager, IHousingPaymentPlanManager housingPaymentPlanManager)
            : base(housingRepository)
        {
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _personManager = personManager;
            _personRepository = personRepository;
            _housingPersonRepository = housingPersonRepository;
            _blockRepository = blockRepository;
            _housingPolicy = housingPolicy;
            _paymentCategoryManager = paymentCategoryManager;
            _housingPaymentPlanManager = housingPaymentPlanManager;
        }

        public override async Task<HousingDto> CreateAsync(CreateHousingDto input)
        {
            CheckCreatePermission();
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            var block = await _blockRepository.GetAsync(input.BlockId);

            var housing = await Housing.CreateAsync(_housingPolicy, SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                block, input.Apartment, housingCategory);

            if (input.CreateTransferForHousingDue.Amount != 0)
            {
                housing = input.CreateTransferForHousingDue.IsDebt
                    ? Housing.IncreaseBalance(housing, input.CreateTransferForHousingDue.Amount)
                    : Housing.DecreaseBalance(housing, input.CreateTransferForHousingDue.Amount);
            }

            await _housingManager.CreateAsync(housing);

            if (input.CreateTransferForHousingDue.Amount != 0)
            {
                var paymentCategory = await _paymentCategoryManager.GetTransferForRegularHousingDueAsync();

                var housingPaymentPlan = input.CreateTransferForHousingDue.IsDebt
                    ? HousingPaymentPlan.CreateDebt(
                        SequentialGuidGenerator.Instance.Create()
                        , AbpSession.GetTenantId()
                        , housing
                        , paymentCategory
                        , input.CreateTransferForHousingDue.Date
                        , input.CreateTransferForHousingDue.Amount
                        , input.CreateTransferForHousingDue.Description
                    )
                    : HousingPaymentPlan.CreateCredit(
                        SequentialGuidGenerator.Instance.Create()
                        , AbpSession.GetTenantId()
                        , housing
                        , paymentCategory
                        , input.CreateTransferForHousingDue.Date
                        , input.CreateTransferForHousingDue.Amount
                        , input.CreateTransferForHousingDue.Description
                        , null
                    );

                await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
            }

            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task<HousingDto> UpdateAsync(UpdateHousingDto input)
        {
            CheckUpdatePermission();
            var existingHousing = await _housingManager.GetAsync(input.Id);
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            var block = await _blockRepository.GetAsync(input.BlockId);

            var housing = await Housing.UpdateAsync(_housingPolicy, existingHousing, block, input.Apartment,
                housingCategory);
            await _housingManager.UpdateAsync(housing);
            return ObjectMapper.Map<HousingDto>(housing);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var housing = await _housingManager.GetAsync(input.Id);
            await _housingManager.DeleteAsync(housing);
        }

        public override async Task<PagedResultDto<HousingDto>> GetAllAsync(PagedHousingResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = (from housing in _housingRepository.GetAll().Include(p => p.HousingCategory)
                        .Include(p => p.Block)
                    join housingPerson in _housingPersonRepository.GetAll() on housing.Id equals housingPerson
                            .HousingId
                        into g1
                    from housingPerson in g1.DefaultIfEmpty()
                    join person in _personRepository.GetAll() on housingPerson.PersonId equals person.Id into g2
                    from person in g2.DefaultIfEmpty()
                    select new {housing, housingPerson, person})
                .WhereIf(input.HousingIds.Count > 0, p => input.HousingIds.Contains(p.housing.Id))
                .WhereIf(input.HousingCategoryIds.Count > 0,
                    p => input.HousingCategoryIds.Contains(p.housing.HousingCategoryId))
                .WhereIf(input.PersonIds.Count > 0,
                    p => input.PersonIds.Contains(p.person != null ? p.person.Id : Guid.Empty));

            var housings = await query.Select(p => p.housing)
                .OrderBy(input.Sorting ?? $"{nameof(HousingDto.Block)} ASC, {nameof(HousingDto.Apartment)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<HousingDto>(query.Count(),
                ObjectMapper.Map<List<HousingDto>>(housings));
        }

        public async Task<List<LookUpDto>> GetHousingLookUpAsync()
        {
            CheckGetAllPermission();
            var query = _housingRepository.GetAll().Include(p => p.Block);

            return
                (from l in await query.ToListAsync()
                    select new LookUpDto(l.Id.ToString(), l.GetName())).ToList();
        }

        public async Task<HousingPersonDto> AddPersonAsync(CreateHousingPersonDto input)
        {
            CheckUpdatePermission();
            var housing = await _housingManager.GetAsync(input.HousingId);
            var person = await _personManager.GetAsync(input.PersonId);

            var housingPerson = await _housingManager.AddPersonAsync(housing, person, input.IsTenant, input.Contact);
            return ObjectMapper.Map<HousingPersonDto>(housingPerson);
        }

        public async Task<List<LookUpDto>> GetPeopleLookUpAsync(Guid housingId)
        {
            CheckGetAllPermission();
            var housing = await _housingRepository.GetAsync(housingId);

            var housingPeople = await _housingPersonRepository.GetAllListAsync(p => p.HousingId == housingId);

            var peopleQuery = _personRepository.GetAll();
            var people = await peopleQuery.WhereIf(housingPeople.Count > 0,
                p => housingPeople.Select(x => x.PersonId).Contains(p.Id) == false).ToListAsync();


            if (people.Count == 0)
            {
                throw new UserFriendlyException("Sistemde uygun kişi bulunamadı.");
            }

            return people.Select(p => new LookUpDto(p.Id.ToString(), $"{p.FirstName} {p.LastName}")).ToList();
        }
    }
}