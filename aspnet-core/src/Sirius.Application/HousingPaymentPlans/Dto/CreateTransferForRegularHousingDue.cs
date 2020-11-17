using System;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class CreateTransferForHousingDueDto
    {
        public Guid HousingId { get; set; }
        public decimal Amount { get; set; }
        public bool IsDebt { get; set; }
        public Guid? PaymentCategoryId { get; set; }

        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}