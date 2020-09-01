using System;
using Abp.Application.Services;
using Sirius.PaymentCategories.Dto;

namespace Sirius.PaymentCategories
{
    public interface IPaymentCategoryAppService : IAsyncCrudAppService<PaymentCategoryDto, Guid, PagedPaymentCategoryResultRequestDto, CreatePaymentCategoryDto, UpdatePaymentCategoryDto>
    {

    }
}
