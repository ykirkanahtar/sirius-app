using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class PaymentPlanForHousingCategoryDto : IShouldNormalize
    {
        public Guid HousingCategoryId { get; set; }
        public decimal AmountPerMonth { get; set; }

        public void Normalize()
        {
            AmountPerMonth = Math.Abs(AmountPerMonth);
        }
    }

    public class CreateHousingPaymentPlanGroupDto
    {
        public string HousingPaymentPlanGroupName { get; set; }
        public int CountOfMonth { get; set; }
        public Guid DefaultToPaymentAccountId { get; set; }
        public int PaymentDayOfMonth { get; set; }
        public string StartDateString { get; set; }
        public string Description { get; set; }
        public ResidentOrOwner ResidentOrOwner { get; set; }

        public List<PaymentPlanForHousingCategoryDto> PaymentPlanForHousingCategories { get; set; }
    }
}