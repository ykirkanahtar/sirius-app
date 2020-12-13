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
using Sirius.People;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanGroupAppService :
        AsyncCrudAppService<HousingPaymentPlanGroup, HousingPaymentPlanGroupDto, Guid,
            PagedHousingPaymentPlanGroupResultRequestDto,
            CreateHousingPaymentPlanGroupDto, UpdateHousingPaymentPlanGroupDto>, IHousingPaymentPlanGroupAppService
    {
        private readonly IRepository<HousingPaymentPlanGroup, Guid> _housingPaymentPlanGroupRepository;
        private readonly IHousingPaymentPlanGroupManager _housingPaymentPlanGroupManager;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IHousingManager _housingManager;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IRepository<Person, Guid> _personRepository;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<HousingPerson, Guid> _housingPersonRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public HousingPaymentPlanGroupAppService(
            IRepository<HousingPaymentPlanGroup, Guid> housingPaymentPlanGroupRepository,
            IHousingPaymentPlanGroupManager housingPaymentPlanGroupManager,
            IHousingPaymentPlanManager housingPaymentPlanManager, IHousingManager housingManager,
            IRepository<Housing, Guid> housingRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository,
            IPaymentCategoryManager paymentCategoryManager, IRepository<Person, Guid> personRepository,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<HousingPerson, Guid> housingPersonRepository,
            IRepository<PaymentCategory, Guid> paymentCategoryRepository, IUnitOfWorkManager unitOfWorkManager)
            : base(housingPaymentPlanGroupRepository)
        {
            _housingPaymentPlanGroupRepository = housingPaymentPlanGroupRepository;
            _housingPaymentPlanGroupManager = housingPaymentPlanGroupManager;
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingManager = housingManager;
            _housingRepository = housingRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _personRepository = personRepository;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingPersonRepository = housingPersonRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task<HousingPaymentPlanGroupDto> CreateAsync(
            CreateHousingPaymentPlanGroupDto input)
        {
            CheckCreatePermission();
            var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
            var housings = await _housingRepository.GetAllListAsync(p => p.HousingCategoryId == housingCategory.Id);
            var paymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();

            var startDate = input.StartDate > Clock.Now ? input.StartDate : Clock.Now;

            if (startDate.Day > input.PaymentDayOfMonth)
            {
                var year = startDate.Year;
                var month = startDate.Month == 12 ? 1 : startDate.Month + 1;
                year = month == 1 ? year + 1 : year;
                startDate = new DateTime(year, month, input.PaymentDayOfMonth);
            }

            var housingPaymentPlanGroup = HousingPaymentPlanGroup.Create(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                input.HousingPaymentPlanGroupName, housingCategory, paymentCategory, input.AmountPerMonth,
                input.CountOfMonth, input.PaymentDayOfMonth
                , input.StartDate, input.Description);

            await _housingPaymentPlanGroupManager.CreateAsync(housingPaymentPlanGroup);

            var housingPaymentPlans = new List<HousingPaymentPlan>();

            foreach (var housing in housings)
            {
                var date = startDate;
                for (var i = 0; i < input.CountOfMonth; i++)
                {
                    if (i > 0)
                    {
                        var day = input.PaymentDayOfMonth;
                        var month = date.Month == 12 ? 1 : date.Month + 1;
                        var year = month == 1 ? date.Year + 1 : date.Year;
                        date = new DateTime(year, month, day);
                    }

                    var housingPaymentPlan = HousingPaymentPlan.CreateDebt(
                        SequentialGuidGenerator.Instance.Create()
                        , AbpSession.GetTenantId()
                        , housingPaymentPlanGroup
                        , housing
                        , paymentCategory
                        , date
                        , input.AmountPerMonth
                        , input.Description
                    );

                    housingPaymentPlans.Add(housingPaymentPlan);
                }
            }

            await _housingPaymentPlanManager.BulkCreateAsync(housingPaymentPlans);
            _housingManager.BulkIncreaseBalance(housings, input.AmountPerMonth * input.CountOfMonth);

            return ObjectMapper.Map<HousingPaymentPlanGroupDto>(housingPaymentPlanGroup);
        }

        public override async Task<HousingPaymentPlanGroupDto> UpdateAsync(UpdateHousingPaymentPlanGroupDto input)
        {
            CheckUpdatePermission();
            var existingHousingPaymentPlanGroup = await _housingPaymentPlanGroupRepository.GetAsync(input.Id);
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

        public override async Task<PagedResultDto<HousingPaymentPlanGroupDto>> GetAllAsync(
            PagedHousingPaymentPlanGroupResultRequestDto input)
        {
            CheckGetAllPermission();

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = (from housingPaymentPlanGroup in _housingPaymentPlanGroupRepository.GetAll()
                            .Include(p => p.HousingCategory)
                            .Include(p => p.PaymentCategory)
                            .Include(p => p.HousingPaymentPlans)
                        join housingCategory in _housingCategoryRepository.GetAll()
                            .Where(p => p.TenantId == AbpSession.TenantId) on housingPaymentPlanGroup
                            .HousingCategoryId equals housingCategory.Id
                        join paymentCategory in _paymentCategoryRepository.GetAll() on housingPaymentPlanGroup
                            .PaymentCategoryId equals paymentCategory.Id
                        join housingPaymentPlan in _housingPaymentPlanRepository.GetAll()
                                .Where(p => p.TenantId == AbpSession.TenantId) on housingPaymentPlanGroup.Id
                            equals housingPaymentPlan.HousingPaymentPlanGroupId
                        join housing in _housingRepository.GetAll().Include(p => p.Block)
                                .Where(p => p.TenantId == AbpSession.TenantId) on housingPaymentPlan
                                .HousingId
                            equals housing.Id
                        join housingPerson in _housingPersonRepository.GetAll()
                                .Where(p => p.TenantId == AbpSession.TenantId) on housing.Id equals housingPerson
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
}