using System;
using Abp.AutoMapper;
using Sirius.Housings;
using Sirius.PaymentCategories;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class CreateTransferForHousingDueInternalUse
    {
        private CreateTransferForHousingDueInternalUse()
        {
            
        }
        
        public CreateTransferForHousingDueInternalUse(Housing housing, decimal amount, bool isDebt,
            PaymentCategory paymentCategory, DateTime date, string description)
        {
            Housing = housing;
            Amount = amount;
            IsDebt = isDebt;
            PaymentCategory = paymentCategory;
            Date = date;
            Description = description;
        }

        public Housing Housing { get; }
        public decimal Amount { get; }
        public bool IsDebt { get; }
        public PaymentCategory PaymentCategory { get; }
        public DateTime Date { get; }
        public string Description { get; }
    }
}