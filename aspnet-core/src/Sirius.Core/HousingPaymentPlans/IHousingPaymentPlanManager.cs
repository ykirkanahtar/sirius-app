using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanManager : IDomainService
    {
        Task BulkCreateAsync(IEnumerable<HousingPaymentPlan> housingPaymentPlans);
        Task CreateAsync(HousingPaymentPlan housingPaymentPlan);
        Task<HousingPaymentPlan>  UpdateAsync(Guid housingPaymentPlanId, DateTime date, decimal amount, string description);
        Task DeleteAsync(HousingPaymentPlan housingPaymentPlan, bool fromAccountBook = false);
        Task<HousingPaymentPlan> GetAsync(Guid id);
    }
}