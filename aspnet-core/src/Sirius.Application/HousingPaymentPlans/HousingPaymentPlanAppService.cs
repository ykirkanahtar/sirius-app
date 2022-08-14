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
using Microsoft.EntityFrameworkCore;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;
using System.Linq.Dynamic.Core;
using Abp.Localization;
using Abp.Localization.Sources;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.PaymentAccounts;
using Sirius.Shared.Constants;
using Sirius.Shared.Dtos;
using Sirius.Shared.Helper;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanAppService :
        AsyncCrudAppService<HousingPaymentPlan, HousingPaymentPlanDto, Guid, PagedHousingPaymentPlanResultRequestDto,
            CreateCreditHousingPaymentPlanDto, UpdateHousingPaymentPlanDto>, IHousingPaymentPlanAppService
    {
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly ILocalizationSource _localizationSource;

        public HousingPaymentPlanAppService(IHousingPaymentPlanManager housingPaymentPlanManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<AccountBook, Guid> accountBookRepository,
            ILocalizationManager localizationManager
        )
            : base(housingPaymentPlanRepository)
        {
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingRepository = housingRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _accountBookRepository = accountBookRepository;
            _localizationSource = localizationManager.GetSource(AppConstants.LocalizationSourceName);
        }

        public async Task<HousingPaymentPlanDto> CreateCreditPaymentAsync(CreateCreditHousingPaymentPlanDto input)
        {
            CheckCreatePermission();
            var housing = await _housingRepository.GetAsync(input.HousingId);
            var paymentCategory = await _paymentCategoryRepository.GetAsync(input.PaymentCategoryId);
            var accountBook = await _accountBookRepository.GetAsync(input.AccountBookId);

            var housingPaymentPlan = HousingPaymentPlan.CreateCredit(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , housing
                , paymentCategory.HousingDueForResidentOrOwner.GetValueOrDefault()
                , paymentCategory
                , input.HousingPaymentPlanDateString.StringToDateTime()
                , input.Amount
                , input.Description
                , accountBook
                , HousingPaymentPlanType.HousingDuePayment
                , null
                , null
            );

            await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
            return ObjectMapper.Map<HousingPaymentPlanDto>(housingPaymentPlan);
        }

        public async Task<HousingPaymentPlanDto> CreateDebtPaymentAsync(CreateDebtHousingPaymentPlanDto input)
        {
            CheckCreatePermission();
            var housing = await _housingRepository.GetAsync(input.HousingId);
            var paymentCategory = await _paymentCategoryRepository.GetAsync(input.PaymentCategoryId);

            var housingPaymentPlan = HousingPaymentPlan.CreateDebt(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , null
                , housing
                , paymentCategory.HousingDueForResidentOrOwner.GetValueOrDefault()
                , paymentCategory
                , input.HousingPaymentPlanDateString.StringToDateTime()
                , input.Amount
                , input.Description
                , HousingPaymentPlanType.HousingDueDefinition
                , null
                , null
            );

            await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
            return ObjectMapper.Map<HousingPaymentPlanDto>(housingPaymentPlan);
        }

        //TODO custom create metotları için IAsyncCrudAppService türemesi olmadan getall metodunun nasıl yazılacağını araştır
        public override Task<HousingPaymentPlanDto> CreateAsync(CreateCreditHousingPaymentPlanDto input)
        {
            return CreateCreditPaymentAsync(input);
        }

        public override async Task<HousingPaymentPlanDto> UpdateAsync(UpdateHousingPaymentPlanDto input)
        {
            CheckUpdatePermission();

            var existingHousingPaymentPlan = await _housingPaymentPlanRepository.GetAsync(input.Id);

            var updatedHousingPaymentPlan = await _housingPaymentPlanManager.UpdateAsync(existingHousingPaymentPlan.Id,
                input.HousingPaymentPlanDateString.StringToDateTime(), input.Amount,
                input.Description);

            return ObjectMapper.Map<HousingPaymentPlanDto>(updatedHousingPaymentPlan);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            var housingPaymentPlan = await _housingPaymentPlanManager.GetAsync(input.Id);
            await _housingPaymentPlanManager.DeleteAsync(housingPaymentPlan);
        }

        public async Task<PagedResultDto<HousingPaymentPlanDto>> GetAllByHousingIdAsync(
            PagedHousingPaymentPlanResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = FilterQuery(input);

            var list = await query
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<HousingPaymentPlanDto>(query.Count(),
                ObjectMapper.Map<List<HousingPaymentPlanDto>>(list));
        }

        private IOrderedQueryable<HousingPaymentPlan> FilterQuery(IHousingPaymentPlanGetAllFilter filter)
        {
            var query = _housingPaymentPlanRepository.GetAll()
                .Where(p => p.HousingId == filter.HousingId)
                .Include(p => p.PaymentCategory)
                .WhereIf(filter.StartDateFilter.HasValue,
                    p => p.Date >= filter.StartDateFilter.Value)
                .WhereIf(filter.EndDateFilter.HasValue,
                    p => p.Date <= filter.EndDateFilter.Value)
                .WhereIf(filter.CreditOrDebtsFilter.Any(), p => filter.CreditOrDebtsFilter.Contains(p.CreditOrDebt))
                .WhereIf(filter.PaymentCategoriesFilter.Any(),
                    p => filter.PaymentCategoriesFilter.Contains(p.PaymentCategoryId ?? Guid.Empty))
                .WhereIf(filter.HousingPaymentPlanTypesFilter.Any(),
                    p => filter.HousingPaymentPlanTypesFilter.Contains(p.HousingPaymentPlanType));

            return query
                .OrderBy(filter.Sorting ?? nameof(HousingPaymentPlan.Date));
        }

        public async Task<List<HousingPaymentPlanExportOutput>> GetAllByHousingIdForExportAsync(
            HousingPaymentPlanGetAllFilter input)
        {
            CheckGetAllPermission();

            var query = FilterQuery(input);

            var list = await query
                .ToListAsync();
            var exportList = ObjectMapper.Map<List<HousingPaymentPlanExportOutput>>(list);
            
            var localizedCreditOrDebtNames = EnumHelper.GetLocalizedEnumNames(typeof(CreditOrDebt), _localizationSource);
            var localizedHousingPaymentPlanTypeNames = EnumHelper.GetLocalizedEnumNames(typeof(HousingPaymentPlanType), _localizationSource);

            exportList.ForEach(p =>
            {
                p.CreditOrDebt = localizedCreditOrDebtNames.Where(x => x.Key == p.CreditOrDebt).Select(x => x.Value)
                    .SingleOrDefault();
                p.HousingPaymentPlanType = localizedHousingPaymentPlanTypeNames.Where(x => x.Key == p.HousingPaymentPlanType).Select(x => x.Value)
                    .SingleOrDefault();
            });

            return exportList;
        }
    }
}