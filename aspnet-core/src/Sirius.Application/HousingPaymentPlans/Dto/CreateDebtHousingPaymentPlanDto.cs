using System;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class CreateDebtHousingPaymentPlanDto : IShouldNormalize
    {
        private IShouldNormalize _shouldNormalizeImplementation;
        public Guid HousingId { get; set; }
        public Guid PaymentCategoryId { get; private set; }
        public string HousingPaymentPlanDateString { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
