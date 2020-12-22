using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Domain.Services;

namespace Sirius.PaymentCategories
{
    public interface IPaymentCategoryManager : IDomainService
    {
        Task<PaymentCategory> GetAsync(Guid id);
        Task CreateAsync(PaymentCategory housing);
        Task UpdateAsync(PaymentCategory housing);
        Task DeleteAsync(PaymentCategory housing);
        Task<PaymentCategory> GetRegularHousingDueAsync();
        Task<PaymentCategory> GetTransferForRegularHousingDueAsync();
        Task<PaymentCategory> GetNettingAsync();
        Task<PaymentCategory> GetTransferForPaymentAccountAsync();
    }
}
