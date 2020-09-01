using System;
using System.Collections.Generic;
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
        Task<List<PaymentCategory>> GetAllAsync(int tenantId, PagedResultRequestDto pagingRequest);
    }
}
