using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks.Dto;
using Sirius.HousingPaymentPlans;
using Sirius.Housings;
using Sirius.PaymentAccounts;
using Sirius.PaymentCategories;

namespace Sirius.AccountBooks
{
    public class AccountBookAppService :
        AsyncCrudAppService<AccountBook, AccountBookDto, Guid, PagedAccountBookResultRequestDto, CreateAccountBookDto,
            UpdateAccountBookDto>, IAccountBookAppService
    {
        private readonly IAccountBookManager _accountBookManager;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IPaymentCategoryManager _paymentCategoryManager;
        private readonly IPaymentAccountManager _paymentAccountManager;
        private readonly IHousingManager _housingManager;
        private readonly IHousingPaymentPlanManager _housingPaymentPlanManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public AccountBookAppService(IAccountBookManager accountBookManager,
            IRepository<AccountBook, Guid> accountBookRepository, IPaymentCategoryManager paymentCategoryManager,
            IUnitOfWorkManager unitOfWorkManager, IHousingManager housingManager,
            IPaymentAccountManager paymentAccountManager, IHousingPaymentPlanManager housingPaymentPlanManager)
            : base(accountBookRepository)
        {
            _accountBookManager = accountBookManager;
            _accountBookRepository = accountBookRepository;
            _paymentCategoryManager = paymentCategoryManager;
            _unitOfWorkManager = unitOfWorkManager;
            _housingManager = housingManager;
            _paymentAccountManager = paymentAccountManager;
            _housingPaymentPlanManager = housingPaymentPlanManager;
        }

        public override Task<AccountBookDto> CreateAsync(CreateAccountBookDto input)
        {
            throw new NotImplementedException();
        }

        public async Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input)
        {
            try
            {
                CheckCreatePermission();
                var housingDuePaymentCategory = await _paymentCategoryManager.GetRegularHousingDueAsync();
                var housing = await _housingManager.GetAsync(input.HousingId);
                var toPaymentAccount = await _paymentAccountManager.GetAsync(input.ToPaymentAccountId);

                var accountBook = AccountBook.CreateHousingDue(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , input.ProcessDateTime
                    , housingDuePaymentCategory.Id
                    , input.HousingId
                    , input.ToPaymentAccountId
                    , input.Amount
                    , input.Description);

                await _accountBookManager.CreateAsync(accountBook);
                await _housingManager.DecreaseBalance(housing, input.Amount);
                await _paymentAccountManager.IncreaseBalance(toPaymentAccount, input.Amount);
                await _housingPaymentPlanManager.CreateAsync(HousingPaymentPlan.CreateCredit(
                    SequentialGuidGenerator.Instance.Create()
                    , AbpSession.GetTenantId()
                    , housing
                    , housingDuePaymentCategory
                    , input.ProcessDateTime
                    , input.Amount
                    , input.Description
                    , accountBook
                ));

                return ObjectMapper.Map<AccountBookDto>(accountBook);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input)
        {
            CheckCreatePermission();
            await _paymentCategoryManager.GetAsync(input.PaymentCategoryId);

            var accountBook = AccountBook.Create(
                SequentialGuidGenerator.Instance.Create()
                , AbpSession.GetTenantId()
                , input.ProcessDateTime
                , input.PaymentCategoryId
                , null
                , input.FromPaymentAccountId
                , input.ToPaymentAccountId
                , input.Amount
                , input.Description
                , input.DocumentDateTime
                , input.DocumentNumber);

            await _accountBookManager.CreateAsync(accountBook);

            if (input.FromPaymentAccountId.HasValue)
            {
                var fromPaymentAccount = await _paymentAccountManager.GetAsync(input.FromPaymentAccountId.Value);
                await _paymentAccountManager.DecreaseBalance(fromPaymentAccount, input.Amount);
            }

            if (input.ToPaymentAccountId.HasValue)
            {
                var toPaymentAccount = await _paymentAccountManager.GetAsync(input.ToPaymentAccountId.Value);
                await _paymentAccountManager.IncreaseBalance(toPaymentAccount, input.Amount);
            }

            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public override async Task<AccountBookDto> UpdateAsync(UpdateAccountBookDto input)
        {
            CheckUpdatePermission();
            var existingAccountBook = await _accountBookRepository.GetAsync(input.Id);
            var accountBook = AccountBook.Update(existingAccountBook, input.Description, input.DocumentDateTime,
                input.DocumentNumber);
            await _accountBookManager.UpdateAsync(accountBook);
            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }

        public override async Task<PagedResultDto<AccountBookDto>> GetAllAsync(PagedAccountBookResultRequestDto input)
        {
            CheckGetAllPermission();
            var housingIdsFromPersonFilter = await _housingManager.GetHousingsFromPersonIds(input.PersonIds);

            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _accountBookRepository.GetAll().Where(p => p.TenantId == AbpSession.TenantId)
                    .Include(p => p.PaymentCategory).Include(p => p.Housing).ThenInclude(p => p.Block).Include(p => p.FromPaymentAccount)
                    .Include(p => p.ToPaymentAccount)
                    .WhereIf(input.StartDate.HasValue, p => p.ProcessDateTime > input.StartDate.Value)
                    .WhereIf(input.EndDate.HasValue, p => p.ProcessDateTime < input.EndDate.Value)
                    .WhereIf(input.HousingIds.Count > 0, p => input.HousingIds.Contains(p.HousingId ?? Guid.Empty))
                    .WhereIf(input.PaymentCategoryIds.Count > 0,
                        p => input.PaymentCategoryIds.Contains(p.PaymentCategoryId))
                    .WhereIf(housingIdsFromPersonFilter.Count > 0,
                        p => housingIdsFromPersonFilter.Select(s => s.Id).Contains(p.HousingId ?? Guid.Empty))
                    .WhereIf(input.FromPaymentAccountIds.Count > 0,
                        p => input.FromPaymentAccountIds.Contains(p.FromPaymentAccountId ?? Guid.Empty))
                    .WhereIf(input.ToPaymentAccountIds.Count > 0,
                        p => input.ToPaymentAccountIds.Contains(p.ToPaymentAccountId ?? Guid.Empty));

                var accountBooks = await query.OrderBy(input.Sorting ?? $"{nameof(AccountBookDto.CreationTime)} DESC")
                    .PageBy(input)
                    .ToListAsync();

                return new PagedResultDto<AccountBookDto>(accountBooks.Count,
                    ObjectMapper.Map<List<AccountBookDto>>(accountBooks));
            }
        }

        // public async Task<AccountBookDto> CreateBankingAndInsuranceTransactionTaxAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateBankingAndIssuranceTransactionTax(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateBankTransferFeeAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateBankTransferFee(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateBillPaymentAsync(CreateBillPaymentAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateBillPayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description
        //         , input.DocumentDateTime
        //         , input.DocumentNumber);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateBonusPaymentAsync(CreateAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateBonusPayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateEftFeeAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateEftFee(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateOtherPayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.HousingId
        //         , input.FromPaymentAccountId
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description
        //         , input.DocumentDateTime
        //         , input.DocumentNumber
        //         );
        //
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateRefundHousingDueAsync(CreateRefundHousingDueAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateRefundHousingDue(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.HousingId
        //         , input.FromPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateSalaryPaymentAsync(CreateAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateSalaryPayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateTransferFromThePreviousPeriodAsync(CreateTransferFromPreviousPeriodAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateTransferFromPreviousPeriod(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateTransferToAdvanceAccountAsync(CreateAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateTransferToAdvanceAccountPayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
        //
        // public async Task<AccountBookDto> CreateWorkerWarmingFeeAsync(CreateAccountBookDto input)
        // {
        //     var accountBook = AccountBook.CreateWorkerWarmingFeePayment(
        //         SequentialGuidGenerator.Instance.Create()
        //         , AbpSession.GetTenantId()
        //         , input.ProcessDateTime
        //         , input.FromPaymentAccountId
        //         , input.ToPaymentAccountId
        //         , input.Amount
        //         , input.Description);
        //     
        //     await _accountBookManager.CreateAsync(accountBook);
        //     return ObjectMapper.Map<AccountBookDto>(accountBook);
        // }
    }
}