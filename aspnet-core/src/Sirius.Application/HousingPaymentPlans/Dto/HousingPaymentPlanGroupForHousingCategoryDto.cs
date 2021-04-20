using System;
using Abp.Runtime.Validation;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class HousingPaymentPlanGroupForHousingCategoryDto : IShouldNormalize
    {
        public Guid HousingCategoryId { get; set; }
        public decimal AmountPerMonth { get; set; }

        public void Normalize()
        {
            AmountPerMonth = Math.Abs(AmountPerMonth);
        }
    }
}