using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace Sirius.HousingPaymentPlans
{
    [Table("AppHousingPaymentPlanGroupHousings")]
    public class HousingPaymentPlanGroupHousing : Entity<Guid>, IMustHaveTenant
    {
        protected HousingPaymentPlanGroupHousing()
        {
        }

        public int TenantId { get; set; }
        public Guid HousingId { get; private set; }
        public decimal AmountPerMonth { get; private set; }
        public Guid HousingPaymentPlanGroupId { get; private set; }
        
        public static HousingPaymentPlanGroupHousing Create(Guid housingPaymentPlanGroupId, Guid housingId, decimal amountPerMonth)
        {
            return new HousingPaymentPlanGroupHousing
            {
                HousingPaymentPlanGroupId = housingPaymentPlanGroupId, 
                HousingId = housingId,
                AmountPerMonth = amountPerMonth
            };
        }
    }
}