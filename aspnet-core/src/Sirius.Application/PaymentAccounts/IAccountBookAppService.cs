using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.AccountBooks.Dto;
using Sirius.PaymentAccounts.Dto;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookAppService : IAsyncCrudAppService<AccountBookDto, Guid,
        PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>
    {
        Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input);

        Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input);

        Task<PagedAccountBookResultDto> GetAllListAsync(PagedAccountBookResultRequestDto input);
    }
} 
