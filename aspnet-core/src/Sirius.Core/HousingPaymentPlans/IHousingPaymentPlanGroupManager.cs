using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanGroupManager : IDomainService
    {
        Task CreateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup);
        Task UpdateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup);
        Task DeleteAsync(HousingPaymentPlanGroup housingPaymentPlanGroup);
        Task<HousingPaymentPlanGroup> GetAsync(Guid id);
    }
}