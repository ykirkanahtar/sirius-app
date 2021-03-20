using System;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class CreateHousingPaymentPlanGroupDto : IShouldNormalize
    {
        public string HousingPaymentPlanGroupName { get; set; }
        public Guid HousingCategoryId { get; set; }
        public decimal AmountPerMonth { get; set; }
        public int CountOfMonth { get; set; }
        public Guid DefaultToPaymentAccountId { get; set; }
        public int PaymentDayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; set; }
        public ResidentOrOwner ResidentOrOwner { get; set; }
        
        public void Normalize()
        {
            AmountPerMonth = Math.Abs(AmountPerMonth);
        }
    }
}