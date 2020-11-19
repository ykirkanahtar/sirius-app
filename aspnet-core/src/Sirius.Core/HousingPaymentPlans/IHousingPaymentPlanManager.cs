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
        Task UpdateAsync(HousingPaymentPlan housingPaymentPlan);
        Task DeleteAsync(HousingPaymentPlan housingPaymentPlan);
        Task<HousingPaymentPlan> GetAsync(Guid id);
    }
}
