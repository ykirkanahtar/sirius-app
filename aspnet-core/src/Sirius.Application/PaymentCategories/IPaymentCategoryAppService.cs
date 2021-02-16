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
        Task<List<LookUpDto>> GetPaymentCategoryLookUpAsync(bool onlyActives);
        Task<List<LookUpDto>> GetHousingDuePaymentCategoryLookUpAsync(bool onlyActives);
        Task<List<LookUpDto>> GetPaymentCategoryForTransferLookUpAsync();
        Task<PaymentCategoryDto> GetRegularHousingDueAsync();
        Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request);
    }
}
