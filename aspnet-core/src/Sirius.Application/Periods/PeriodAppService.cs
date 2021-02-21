using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.EntityFrameworkCore.Repositories;
using Sirius.HousingPaymentPlans;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;
using Sirius.Periods.Dto;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.Periods
{
    public class PeriodAppService :
        AsyncCrudAppService<Period, PeriodDto, Guid, PagedPeriodResultRequestDto, CreatePeriodForSiteDto,
            UpdatePeriodDto>,
        IPeriodAppService
    {
        private readonly IRepository<Period, Guid> _periodRepository;
        private readonly IPeriodManager _periodManager;
        private readonly IBlockManager _blockManager;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IHousingPaymentPlanGroupManager _housingPaymentPlanGroupManager;
        public PeriodAppService(IRepository<Period, Guid> periodRepository, IPeriodManager periodManager,
            IBlockManager blockManager, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IPaymentCategoryManager paymentCategoryManager, IPaymentAccountManager paymentAccountManager, IRepository<HousingCategory, Guid> housingCategoryRepository, IRepository<Housing, Guid> housingRepository, IHousingPaymentPlanGroupManager housingPaymentPlanGroupManager) : base(
            periodRepository)
        {
            _periodRepository = periodRepository;
            _periodManager = periodManager;
            _blockManager = blockManager;
            _paymentCategoryRepository = paymentCategoryRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _paymentAccountManager = paymentAccountManager;
            _housingCategoryRepository = housingCategoryRepository;
            _housingRepository = housingRepository;
            _housingPaymentPlanGroupManager = housingPaymentPlanGroupManager;
        }

        public async Task<PeriodDto> CreateForSiteAsync(CreatePeriodForSiteDto input)
        {
            CheckCreatePermission();

            var period = Period.CreateForSite(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.Name
                , input.StartDate
            );

            await CreateAsync(period, input.DefaultPaymentAccountIdForRegularHousingDue, input.RegularHousingDueName,
                input.PaymentCategories, input.HousingPaymentPlanGroups);

            return ObjectMapper.Map<PeriodDto>(period);
        }

        public async Task<PeriodDto> CreateForBlockAsync(CreatePeriodForBlockDto input)
        {
            CheckCreatePermission();

            var block = await _blockManager.GetAsync(input.BlockId);

            var period = Period.CreateForBlock(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.Name
                , input.StartDate
                , block
            );

            await CreateAsync(period, input.DefaultPaymentAccountIdForRegularHousingDue, input.RegularHousingDueName,
                input.PaymentCategories, input.HousingPaymentPlanGroups);

            return ObjectMapper.Map<PeriodDto>(period);
        }

        private async Task CreateAsync(Period period, Guid defaultPaymentAccountIdForRegularHousingDue,
            string regularHousingDueName, List<Guid> paymentCategories, List<CreateHousingPaymentPlanGroupDto> housingPaymentPlanGroups)
        {
            await _periodManager.CreateAsync(period);
            
            /*Yeni dönem için olağan aidat ödemesi için ödeme kategorisi oluşturuluyor*/
            var defaultPaymentAccountForRegularHousingDue =
                await _paymentAccountManager.GetAsync(defaultPaymentAccountIdForRegularHousingDue);

            var housingDuePaymentCategory = PaymentCategory.Create(
                id: SequentialGuidGenerator.Instance.Create(),
                tenantId: AbpSession.GetTenantId(),
                paymentCategoryName: regularHousingDueName,
                housingDueType: HousingDueType.RegularHousingDue,
                isValidForAllPeriods: false,
                defaultFromPaymentAccountId: null,
                defaultToPaymentAccountId: defaultPaymentAccountForRegularHousingDue.Id,
                showInLists: false);
            await _paymentCategoryManager.CreateAsync(housingDuePaymentCategory);
            /******************************************************************/

            /*Yeni dönem için konut ödeme planları oluşturuluyor ve olağan aidat ödemesi için geçmiş dönemden devirler yapılıyor*/
            foreach (var input in housingPaymentPlanGroups)  
            {
                var housingCategory = await _housingCategoryRepository.GetAsync(input.HousingCategoryId);
                var housings = await _housingRepository.GetAllListAsync(p => p.HousingCategoryId == housingCategory.Id);

                var housingPaymentPlanGroup = HousingPaymentPlanGroup.Create(SequentialGuidGenerator.Instance.Create(),
                    AbpSession.GetTenantId(),
                    input.HousingPaymentPlanGroupName, housingCategory, housingDuePaymentCategory, input.AmountPerMonth,
                    input.CountOfMonth, input.PaymentDayOfMonth
                    , input.StartDate, input.Description);
                
                await _housingPaymentPlanGroupManager.CreateAsync(housingPaymentPlanGroup, housings, input.StartDate,
                    housingDuePaymentCategory, true, period.StartDate);
            }
            /*******************************************************************************************************/

            
            await SetPassivePaymentCategoriesAsync(paymentCategories);
        }

        private async Task SetPassivePaymentCategoriesAsync(List<Guid> activePaymentCategories)
        {
            var allPaymentCategories = await _paymentCategoryRepository.GetAll()
                .Where(p => p.IsActive && p.IsValidForAllPeriods == false &&
                            activePaymentCategories.Contains(p.Id) == false).ToListAsync();

            foreach (var paymentCategory in allPaymentCategories)
            {
                paymentCategory.SetPassive();
            }
        }

        public override Task<PeriodDto> CreateAsync(CreatePeriodForSiteDto input)
        {
            CheckCreatePermission();
            throw new NotSupportedException();
        }

        public override async Task<PeriodDto> UpdateAsync(UpdatePeriodDto input)
        {
            CheckUpdatePermission();
            var existingPeriod = await _periodManager.GetAsync(input.Id);
            var period = Period.Update(existingPeriod, input.Name);
            await _periodManager.UpdateAsync(period);
            return ObjectMapper.Map<PeriodDto>(period);
        }

        public override Task DeleteAsync(EntityDto<Guid> input)
        {
            CheckDeletePermission();
            throw new NotSupportedException();
        }

        public override async Task<PagedResultDto<PeriodDto>> GetAllAsync(PagedPeriodResultRequestDto input)
        {
            CheckGetAllPermission();
            var query = _periodRepository.GetAll().Where(p => p.PeriodFor == input.PeriodFor)
                .WhereIf(!string.IsNullOrWhiteSpace(input.Name),
                    p => p.Name == input.Name)
                .WhereIf(input.BlockId.HasValue,
                    p => p.BlockId == input.BlockId.Value);

            var periods = await query
                .OrderBy(input.Sorting ?? $"{nameof(PeriodDto.Name)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<PeriodDto>(query.Count(),
                ObjectMapper.Map<List<PeriodDto>>(periods));
        }

        public async Task<List<LookUpDto>> GetPeriodLookUpAsync()
        {
            CheckGetAllPermission();

            var period = await _periodRepository.GetAllListAsync();

            return
                (from l in period
                    select new LookUpDto(l.Id.ToString(), l.Name)).ToList();
        }
    }
}