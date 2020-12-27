using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.AccountBooks.Dto;

namespace Sirius.AccountBooks
{
    public interface IAccountBookAppService : IAsyncCrudAppService<AccountBookDto, Guid,
        PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>
    {
        Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input);

        Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input);

        Task<PagedAccountBookResultDto> GetAllListAsync(PagedAccountBookResultRequestDto input);
    }
} 
