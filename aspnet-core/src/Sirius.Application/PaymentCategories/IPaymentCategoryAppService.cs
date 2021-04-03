using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Dtos;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories
{
    public interface IPaymentCategoryAppService : IAsyncCrudAppService<PaymentCategoryDto, Guid, PagedPaymentCategoryResultRequestDto, CreatePaymentCategoryDto, UpdatePaymentCategoryDto>
    {
        Task<PaymentCategoryDto> CreateIncomeAsync(CreateIncomePaymentCategoryDto input);
        Task<PaymentCategoryDto> CreateExpenseAsync(CreateExpensePaymentCategoryDto input);
        Task<PaymentCategoryDto> CreateTransferAsync(CreateTransferPaymentCategoryDto input);
        Task<List<PaymentCategoryDto>> GetPaymentCategoryForMenuAsync();
        Task<List<LookUpDto>> GetLookUp(bool onlyActives);
        Task<List<LookUpDto>> GetLookUpByPaymentCategoryType(bool onlyActives, PaymentCategoryType paymentCategoryType);
        Task<List<LookUpDto>> GetPaymentCategoryForTransferLookUpAsync();
        Task<List<LookUpDto>> GetLookUpByHousingId(Guid housingId);
        Task<List<LookUpDto>> GetHousingDueLookUp();
        Task<List<string>> GetPaymentCategoryFromAutoCompleteFilterAsync(string request);
    }
}
