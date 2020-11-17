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
using Sirius.AccountBooks;
using Sirius.HousingCategories;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;
using System.Linq.Dynamic.Core;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanAppService :
        AsyncCrudAppService<HousingPaymentPlan, HousingPaymentPlanDto, Guid, PagedHousingPaymentPlanResultRequestDto,
            CreateCreditHousingPaymentPlanDto, UpdateHousingPaymentPlanDto>, IHousingPaymentPlanAppService
    {
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IHousingManager _housingManager;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<HousingCategory, Guid> _housingCategoryRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public HousingPaymentPlanAppService(IHousingPaymentPlanManager housingPaymentPlanManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository,
            IPaymentCategoryManager paymentCategoryManager, IHousingManager housingManager,
            IUnitOfWorkManager unitOfWorkManager)
            : base(housingPaymentPlanRepository)
        {
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingRepository = housingRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _accountBookRepository = accountBookRepository;
            _housingCategoryRepository = housingCategoryRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _housingManager = housingManager;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task CreateTransferForHousingDueAsync(
            CreateTransferForHousingDueDto input)
        {
            CheckCreatePermission();
            var housing = await _housingRepository.GetAsync(input.HousingId);
            var paymentCategory = input.PaymentCategoryId.HasValue
                ? await _paymentCategoryRepository.GetAsync(input.PaymentCategoryId.Value)
                : await _paymentCategoryManager.GetTransferForRegularHousingDueAsync();

            if (paymentCategory.HousingDueType != HousingDueType.TransferForRegularHousingDue)
                throw new Exception("Kritik hata! Ödeme türü kategorisi devir tipinde olmalıdır.");

            var housingPaymentPlan = input.IsDebt
                ? HousingPaymentPlan.CreateDebt(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , housing
                    , paymentCategory
                    , input.Date
                    , input.Amount
                    , input.Description
                )
                : HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , housing
                    , paymentCategory
                    , input.Date
                    , input.Amount
                    , input.Description
                    , null
                );

            await _housingPaymentPlanManager.CreateAsync(housingPaymentPlan);
            if (input.IsDebt)
            {
                await _housingManager.IncreaseBalance(housing, input.Amount);
            }
            else
            {
                await _housingManager.DecreaseBalance(housing, input.Amount);
            }
        }

        public async Task CreateDebtPaymentForHousingCategoryAsync(
            CreateDebtHousingPaymentPlanForHousingCategoryDto input)
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
                , paymentCategory
                , input.Date
                , input.Amount
                , input.Description
                , accountBook
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
                , housing
                , paymentCategory
                , input.Date
                , input.Amount
                , input.Description
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
            var housingPaymentPlan = HousingPaymentPlan.Update(existingHousingPaymentPlan, input.Date,
                input.Amount, input.Description);
            await _housingPaymentPlanManager.UpdateAsync(housingPaymentPlan);
            return ObjectMapper.Map<HousingPaymentPlanDto>(housingPaymentPlan);
        }

        public async Task<PagedResultDto<HousingPaymentPlanDto>> GetAllByHousingIdAsync(
            PagedHousingPaymentPlanResultRequestDto input)
        {
            CheckGetAllPermission();
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _housingPaymentPlanRepository.GetAll()
                    .Where(p => p.TenantId == AbpSession.TenantId && p.HousingId == input.HousingId)
                    .Include(p => p.PaymentCategory);

                var list = await query
                    .OrderBy(input.Sorting ?? "Date")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<HousingPaymentPlanDto>(query.Count(),
                    ObjectMapper.Map<List<HousingPaymentPlanDto>>(list));
            }
        }
    }
}