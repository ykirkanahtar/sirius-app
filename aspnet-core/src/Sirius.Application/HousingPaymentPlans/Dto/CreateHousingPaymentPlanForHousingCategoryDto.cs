using System;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class CreateDebtHousingPaymentPlanForHousingCategoryDto
    {
        public Guid HousingCategoryId { get; set; }
        public decimal AmountPerMonth { get; set; }
        public int CountOfMonth { get; set; }
        
        public int PaymentDayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; set; }
    }
}