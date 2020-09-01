using System;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.AccountBooks;
using Sirius.HousingPaymentPlans.Dto;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.HousingPaymentPlans
{
    public class HousingPaymentPlanAppService : AsyncCrudAppService<HousingPaymentPlan, HousingPaymentPlanDto, Guid, PagedHousingPaymentPlanResultRequestDto, CreateCreditHousingPaymentPlanDto, UpdateHousingPaymentPlanDto>,  IHousingPaymentPlanAppService
    {
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IRepository<HousingPaymentPlan, Guid> _housingPaymentPlanRepository;
        private readonly IRepository<Housing, Guid> _housingRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        public HousingPaymentPlanAppService(IHousingPaymentPlanManager housingPaymentPlanManager, IRepository<HousingPaymentPlan, Guid> housingPaymentPlanRepository, IRepository<Housing, Guid> housingRepository, IRepository<PaymentCategory, Guid> paymentCategoryRepository, IRepository<AccountBook, Guid> accountBookRepository)
            : base(housingPaymentPlanRepository)
        {
            _housingPaymentPlanManager = housingPaymentPlanManager;
            _housingPaymentPlanRepository = housingPaymentPlanRepository;
            _housingRepository = housingRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
            _accountBookRepository = accountBookRepository;
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
