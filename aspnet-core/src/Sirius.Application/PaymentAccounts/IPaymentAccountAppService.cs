using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.PaymentAccounts.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.PaymentAccounts
{
    public interface IPaymentAccountAppService : IAsyncCrudAppService<PaymentAccountDto, Guid, PagedPaymentAccountResultRequestDto, CreateCashAccountDto, UpdatePaymentAccountDto>
    {
        Task<PaymentAccountDto> CreateCashAccountAsync(CreateCashAccountDto input);
        Task<PaymentAccountDto> CreateBankAccountAsync(CreateBankAccountDto input);
        Task<List<LookUpDto>> GetPaymentAccountLookUpAsync();
        Task<PaymentAccountDto> GetDefaultPaymentAccountAsync();
    }
}
