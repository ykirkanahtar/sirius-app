using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.AccountBooks.Dto;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories.Dto;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookAppService : IAsyncCrudAppService<AccountBookDto, Guid,
        PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>
    {
        Task<AccountBookDto> CreateHousingDueAsync(CreateHousingDueAccountBookDto input);

        Task<AccountBookDto> CreateOtherPaymentAsync(CreateOtherPaymentAccountBookDto input);

        Task<PagedAccountBookResultDto> GetAllListAsync(PagedAccountBookResultRequestDto input);
        Task<List<PaymentCategoryLookUpDto>> GetPaymentCategoryLookUpForEditAccountBookAsync(Guid accountBookId);
    }
} 
