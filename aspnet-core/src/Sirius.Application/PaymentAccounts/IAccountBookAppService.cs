using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.AccountBooks.Dto;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.PaymentAccounts
{
    public interface IAccountBookAppService : IAsyncCrudAppService<AccountBookDto, Guid,
        PagedAccountBookResultRequestDto, CreateAccountBookDto, UpdateAccountBookDto>
    {
        Task<PagedAccountBookResultDto> GetAllListAsync(PagedAccountBookResultRequestDto input);

        Task<List<LookUpDto>> GetPaymentCategoryLookUpForEditAccountBookAsync(
            Guid accountBookId);
    }
} 
