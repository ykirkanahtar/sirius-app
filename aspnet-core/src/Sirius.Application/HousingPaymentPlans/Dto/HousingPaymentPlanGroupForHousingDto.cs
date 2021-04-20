using System;
using Abp.Runtime.Validation;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class HousingPaymentPlanGroupForHousingDto : IShouldNormalize
    {
        public Guid HousingId { get; set; }
        public decimal AmountPerMonth { get; set; }

        public void Normalize()
        {
            AmountPerMonth = Math.Abs(AmountPerMonth);
        }
    }
}