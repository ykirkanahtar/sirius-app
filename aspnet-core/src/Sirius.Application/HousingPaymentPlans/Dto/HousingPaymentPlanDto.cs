using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlan))]
    public class HousingPaymentPlanDto : FullAuditedEntityDto<Guid>
    {
        public Guid HousingId { get; private set; }
        public Guid PaymentCategoryId { get; private set; }
        public DateTime Date { get; private set; }
        public PaymentPlanType PaymentPlanType { get; private set; }
        public decimal Amount { get; private set; }
        public string Description { get; private set; }
        public Guid? AccountBookId { get; private set; }
    }
}
