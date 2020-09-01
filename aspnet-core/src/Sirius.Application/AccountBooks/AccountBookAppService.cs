using System;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sirius.AccountBooks.Dto;
using Sirius.PaymentCategories;
using Sirius.Shared.Constants;

namespace Sirius.AccountBooks
{
    public class AccountBookAppService : AsyncCrudAppService<AccountBook, AccountBookDto, Guid, PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>,  IAccountBookAppService
    {
        private readonly IAccountBookManager _accountBookManager;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;
        private readonly IRepository<PaymentCategory, Guid> _paymentCategoryRepository;

        public AccountBookAppService(IAccountBookManager accountBookManager, IRepository<AccountBook, Guid> accountBookRepository, IGuidGenerator guidGenerator, IRepository<PaymentCategory, Guid> paymentCategoryRepository)
            :base(accountBookRepository)
        {
            _accountBookManager = accountBookManager;
            _accountBookRepository = accountBookRepository;
            _paymentCategoryRepository = paymentCategoryRepository;
        }
        
        public override Task<AccountBookDto> CreateAsync(CreateAccountBookDto input)
        {
            throw new NotImplementedException();
        }
        
        public async Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input)
        {
            var housingDuePaymentCategory = await _paymentCategoryRepository
                .GetAll().Where(p => p.PaymentCategoryName == AppConstants.HousingDueString).SingleAsync();
            
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
            return ObjectMapper.Map<AccountBookDto>(accountBook);
        }
        
        public override async Task<AccountBookDto> UpdateAsync(UpdateAccountBookDto input)
        {
            var existingAccountBook = await _accountBookRepository.GetAsync(input.Id);
            var accountBook = AccountBook.Update(existingAccountBook, input.Description, input.DocumentDateTime,
                input.DocumentNumber);
            await _accountBookManager.UpdateAsync(accountBook);
            return ObjectMapper.Map<AccountBookDto>(accountBook);            
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
