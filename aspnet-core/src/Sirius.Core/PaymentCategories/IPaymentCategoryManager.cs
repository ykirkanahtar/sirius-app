﻿using System;
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
        Task CreateAsync(PaymentCategory paymentCategory);
        Task UpdateAsync(PaymentCategory paymentCategory);
        Task DeleteAsync(PaymentCategory paymentCategory);
        Task<List<Guid>> GetHousingCategories(Guid paymentCategoryId);
        Task<List<Guid>> GetPaymentCategoriesByHousingCategoryIds(List<Guid> housingCategoryIds);
    }
}
