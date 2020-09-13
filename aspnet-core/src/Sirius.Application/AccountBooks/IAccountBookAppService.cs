using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.AccountBooks.Dto;

namespace Sirius.AccountBooks
{
    public interface IAccountBookAppService : IAsyncCrudAppService<AccountBookDto, Guid, PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>
    {
        Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input);
        // Task<AccountBookDto> CreateBillPaymentAsync(CreateBillPaymentAccountBookDto input);
        // Task<AccountBookDto> CreateTransferFromThePreviousPeriodAsync(CreateTransferFromPreviousPeriodAccountBookDto input);
        // Task<AccountBookDto> CreateSalaryPaymentAsync(CreateAccountBookDto input);
        // Task<AccountBookDto> CreateBankTransferFeeAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input);
        // Task<AccountBookDto> CreateEftFeeAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input);
        // Task<AccountBookDto> CreateBankingAndInsuranceTransactionTaxAsync(CreateBankTransferOrEftOrBankTaxFeeAccountBookDto input);
        // Task<AccountBookDto> CreateBonusPaymentAsync(CreateAccountBookDto input);
        // Task<AccountBookDto> CreateWorkerWarmingFeeAsync(CreateAccountBookDto input);
        // Task<AccountBookDto> CreateTransferToAdvanceAccountAsync(CreateAccountBookDto input);
        // Task<AccountBookDto> CreateRefundHousingDueAsync(CreateRefundHousingDueAccountBookDto input);
        // Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input);
    }
} 
