using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace Sirius.HousingPaymentPlans
{
    [Table("AppHousingPaymentPlanGroupHousingCategories")]
    public class HousingPaymentPlanGroupHousingCategory : Entity<Guid>, IMustHaveTenant
    {
        protected HousingPaymentPlanGroupHousingCategory()
        {
        }

        public int TenantId { get; set; }
        public Guid HousingCategoryId { get; private set; }
        public decimal AmountPerMonth { get; private set; }
        public Guid HousingPaymentPlanGroupId { get; private set; }
        
        public static HousingPaymentPlanGroupHousingCategory Create(Guid housingPaymentPlanGroupId, Guid housingCategoryId, decimal amountPerMonth)
        {
            return new HousingPaymentPlanGroupHousingCategory
            {
                HousingPaymentPlanGroupId = housingPaymentPlanGroupId, 
                HousingCategoryId = housingCategoryId,
                AmountPerMonth = amountPerMonth
            };
        }
    }
}