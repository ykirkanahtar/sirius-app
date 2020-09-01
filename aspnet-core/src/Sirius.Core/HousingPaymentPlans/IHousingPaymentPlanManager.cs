using System;
using System.Threading.Tasks;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanManager
    {
        Task<HousingPaymentPlan> GetAsync(Guid id);
        Task CreateAsync(HousingPaymentPlan housingPaymentPlan);
        Task UpdateAsync(HousingPaymentPlan housingPaymentPlan);
    }
}
