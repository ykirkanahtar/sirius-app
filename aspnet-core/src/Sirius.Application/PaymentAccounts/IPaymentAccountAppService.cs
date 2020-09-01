using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.AccountBooks.Dto;
using Sirius.PaymentAccounts.Dto;

namespace Sirius.PaymentAccounts
{
    public interface IPaymentAccountAppService : IAsyncCrudAppService<PaymentAccountDto, Guid, PagedPaymentAccountResultRequestDto, CreateCashAccountDto, UpdatePaymentAccountDto>
    {
        Task<PaymentAccountDto> CreateCashAccountAsync(CreateCashAccountDto input);
        Task<PaymentAccountDto> CreateBankAccountAsync(CreateBankOrAdvanceAccountDto input);
        Task<PaymentAccountDto> CreateAdvanceAccountAsync(CreateBankOrAdvanceAccountDto input);
    }
}
