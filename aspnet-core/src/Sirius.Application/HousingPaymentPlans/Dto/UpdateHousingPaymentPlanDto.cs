using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlan))]
    public class UpdateHousingPaymentPlanDto : FullAuditedEntityDto<Guid>
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}