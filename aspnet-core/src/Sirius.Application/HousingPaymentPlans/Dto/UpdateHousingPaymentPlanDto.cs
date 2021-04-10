using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    public class UpdateHousingPaymentPlanDto : FullAuditedEntityDto<Guid>
    {
        public string HousingPaymentPlanDateString { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}