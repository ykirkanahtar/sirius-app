using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.PaymentCategories
{
    public interface IPaymentCategoryAppService : IAsyncCrudAppService<PaymentCategoryDto, Guid, PagedPaymentCategoryResultRequestDto, CreatePaymentCategoryDto, UpdatePaymentCategoryDto>
    {
        Task<List<LookUpDto>> GetPaymentCategoryLookUpAsync();
        Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request);
    }
}
