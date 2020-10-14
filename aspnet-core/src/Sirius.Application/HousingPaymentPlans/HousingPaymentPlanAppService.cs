using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.Timing;
using Sirius.AccountBooks;
using Sirius.HousingCategories;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentCategories;

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

        public HousingPaymentPlanAppService(IHousingPaymentPlanManager housingPaymentPlanManager,
            IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository,
            IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository,
            IRepository<AccountBook, Guid> accountBookRepository,
            IRepository<HousingCategory, Guid> housingCategoryRepository,
            IPaymentCategoryManager paymentCategoryManager, IHousingManager housingManager)
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
        }

        public async Task CreateDebtPaymentForHousingCategory(
            CreateDebtHousingPaymentPlanForHousingCategoryDto input)
        {
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

            try
            {
                await _housingPaymentPlanManager.BulkCreateAsync(housingPaymentPlans);
                _housingManager.BulkIncreaseBalance(housings, input.AmountPerMonth * input.CountOfMonth);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<HousingPaymentPlanDto> CreateCreditPaymentAsync(CreateCreditHousingPaymentPlanDto input)
        {
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
            var existingHousingPaymentPlan = await _housingPaymentPlanRepository.GetAsync(input.Id);
            var housingPaymentPlan = HousingPaymentPlan.Update(existingHousingPaymentPlan, input.Date,
                input.Amount, input.Description);
            await _housingPaymentPlanManager.UpdateAsync(housingPaymentPlan);
            return ObjectMapper.Map<HousingPaymentPlanDto>(housingPaymentPlan);
        }
    }
}