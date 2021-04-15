using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.HousingPaymentPlans
{
    public interface IHousingPaymentPlanGroupManager : IDomainService
    {
        Task CreateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup,
            DateTime startDate, PaymentCategory paymentCategory, bool transferFromPreviousPeriod,
            DateTime? periodStartDate);
        Task UpdateAsync(HousingPaymentPlanGroup housingPaymentPlanGroup);
        Task DeleteAsync(HousingPaymentPlanGroup housingPaymentPlanGroup);
        Task<HousingPaymentPlanGroup> GetAsync(Guid id);
        DateTime GetStartDate(DateTime startDate, int paymentDayOfMonth);
    }
}