using System;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]

    public class CreateCreditHousingPaymentPlanDto : IShouldNormalize
    {
        public Guid HousingId { get; set; }
        public Guid PaymentCategoryId { get; private set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public Guid AccountBookId { get; set; }
        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
