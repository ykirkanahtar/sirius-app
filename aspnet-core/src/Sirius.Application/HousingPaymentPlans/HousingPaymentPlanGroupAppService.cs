using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentCategories;
using System.Linq.Dynamic.Core;
using Abp.UI;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.PaymentAccounts;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanGroupAppService :
        AsyncCrudAppService<HousingPaymentPlanGroup, HousingPaymentPlanGroupDto, Guid,
            PagedHousingPaymentPlanGroupResultRequestDto,
            CreateHousingPaymentPlanGroupDto, UpdateHousingPaymentPlanGroupDto>, IHousingPaymentPlanGroupAppService
    {
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IHousingPaymentPlanGroupManager _housingPaymentPlanGroupManager;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IRepository<PaymentAccount, Guid> _paymentAccountRepository;

        public HousingPaymentPlanGroupAppService(
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IHousingPaymentPlanGroupManager housingPaymentPlanGroupManager,
            IRepository<Housing, Guid> housingRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository,
            IPaymentCategoryManager paymentCategoryManager, IRepository<Person, Guid> personRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<HousingPerson, Guid> housingPersonRepository,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository, 
            IRepository<PaymentAccount, Guid> paymentAccountRepository)
            : base(housingPaymentPlanGroupRepository)
        {
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingPaymentPlanGroupManager = housingPaymentPlanGroupManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _personRepository = personRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPersonRepository = housingPersonRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _paymentAccountRepository = paymentAccountRepository;
        }

        public override async Task<HousingPaymentPlanGroupDto> CreateAsync(
            CreateHousingPaymentPlanGroupDto input)
        {
            CheckCreatePermission();
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            var housings = await _housingRepository.GetAllListAsync(p => p.HousingCategoryId == housingCategory.Id);

            var paymentCategory = PaymentCategory.CreateHousingDue(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(), input.HousingPaymentPlanGroupName, /*input.HousingDueType,*/
                input.DefaultToPaymentAccountId);

            await _paymentCategoryManager.CreateAsync(paymentCategory);

            var housingPaymentPlanGroup = HousingPaymentPlanGroup.Create(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                input.HousingPaymentPlanGroupName, housingCategory, paymentCategory, input.AmountPerMonth,
                input.CountOfMonth, input.PaymentDayOfMonth
                , input.StartDate, input.Description);

            await _housingPaymentPlanGroupManager.CreateAsync(housingPaymentPlanGroup, housings, input.StartDate,
                paymentCategory, false, null);

            return ObjectMapper.Map<HousingPaymentPlanGroupDto>(housingPaymentPlanGroup);
        }

        public override async Task<HousingPaymentPlanGroupDto> UpdateAsync(UpdateHousingPaymentPlanGroupDto input)
        {
            CheckUpdatePermission();
            var existingHousingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAsync(input.Id);

            var paymentCategory =
                await _paymentCategoryRepository.GetAsync(existingHousingPaymentPlanGroup.PaymentCategoryId);

            if (input.DefaultToPaymentAccountId != paymentCategory.DefaultToPaymentAccountId)
            {
                var paymentAccount = await _paymentAccountRepository.GetAsync(input.DefaultToPaymentAccountId);
                paymentCategory.SetDefaultToPaymentAccount(paymentAccount);
            }

            var housingPaymentPlanGroup = HousingPaymentPlanGroup.Update(existingHousingPaymentPlanGroup,
                input.HousingPaymentPlanGroupName);
            
            await _housingPaymentPlanGroupManager.UpdateAsync(housingPaymentPlanGroup);
            return ObjectMapper.Map<HousingPaymentPlanGroupDto>(housingPaymentPlanGroup);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var housingPaymentPlanGroup = await _housingPaymentPlanGroupManager.GetAsync(input.Id);
            await _housingPaymentPlanGroupManager.DeleteAsync(housingPaymentPlanGroup);
        }

        public async Task<UpdateHousingPaymentPlanGroupDto> GetForUpdate(Guid id)
        {
            CheckGetPermission();
            var existingHousingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAsync(id);

            var paymentCategory =
                await _paymentCategoryRepository.GetAsync(existingHousingPaymentPlanGroup.PaymentCategoryId);

            return new UpdateHousingPaymentPlanGroupDto //TODO object mapper'a taşıs
            {
                Id = id,
                CreationTime = existingHousingPaymentPlanGroup.CreationTime,
                CreatorUserId = existingHousingPaymentPlanGroup.CreatorUserId,
                IsDeleted = existingHousingPaymentPlanGroup.IsDeleted,
                LastModificationTime = existingHousingPaymentPlanGroup.LastModificationTime,
                LastModifierUserId = existingHousingPaymentPlanGroup.LastModifierUserId,
                DefaultToPaymentAccountId = paymentCategory.DefaultToPaymentAccountId.GetValueOrDefault(),
                HousingPaymentPlanGroupName = existingHousingPaymentPlanGroup.HousingPaymentPlanGroupName
            };
        }
        
        public override async Task<PagedResultDto<HousingPaymentPlanGroupDto>> GetAllAsync(
            PagedHousingPaymentPlanGroupResultRequestDto input)
        {
            CheckGetAllPermission();

            var query = (from housingPaymentPlanGroup in _housingPaymentPlanGroupRepository.GetAll()
                        .Include(p => p.HousingCategory)
                        .Include(p => p.PaymentCategory)
                        .Include(p => p.HousingPaymentPlans)
                    join housingCategory in _housingCategoryRepository.GetAll() on housingPaymentPlanGroup
                        .HousingCategoryId equals housingCategory.Id
                    join paymentCategory in _paymentCategoryRepository.GetAll() on housingPaymentPlanGroup
                        .PaymentCategoryId equals paymentCategory.Id
                    join housingPaymentPlan in _housingPaymentPlanRepository.GetAll() on housingPaymentPlanGroup.Id
                        equals housingPaymentPlan.HousingPaymentPlanGroupId
                    join housing in _housingRepository.GetAll().Include(p => p.Block) on housingPaymentPlan
                            .HousingId
                        equals housing.Id
                    join housingPerson in _housingPersonRepository.GetAll() on housing.Id equals housingPerson
                            .HousingId
                        into g1
                    from housingPerson in g1.DefaultIfEmpty()
                    join person in _personRepository.GetAll().Where(p => p.TenantId == AbpSession.TenantId) on
                        housingPerson.PersonId equals person.Id into g2
                    from person in g2.DefaultIfEmpty()
                    select new
                    {
                        housingPaymentPlanGroup, housingCategory, paymentCategory, housingPaymentPlan, housing,
                        housingPerson, person
                    })
                .WhereIf(input.HousingIds.Count > 0, p => input.HousingIds.Contains(p.housing.Id))
                .WhereIf(input.HousingCategoryIds.Count > 0,
                    p => input.HousingCategoryIds.Contains(p.housingCategory.Id))
                .WhereIf(input.PersonIds.Count > 0,
                    p => input.PersonIds.Contains(p.person != null ? p.person.Id : Guid.Empty));

            //TODO linq sorgusu düzeltilecek
            var list = (await query.ToListAsync())
                .AsQueryable()
                .GroupBy(p => p.housingPaymentPlanGroup)
                .Select(p => p.Key)
                .OrderBy(input.Sorting ?? $"{nameof(HousingPaymentPlanGroupDto.StartDate)} ASC");

            var housingPaymentPlanGroups = list
                .PageBy(input)
                .ToList();

            // var housingPaymentPlanGroups = await query
            //     .OrderBy(input.Sorting ?? $"{nameof(HousingPaymentPlanGroupDto.StartDate)} ASC")
            //     .PageBy(input)
            //     .ToListAsync();

            return new PagedResultDto<HousingPaymentPlanGroupDto>(query.Count(),
                ObjectMapper.Map<List<HousingPaymentPlanGroupDto>>(housingPaymentPlanGroups));
        }
    }
}