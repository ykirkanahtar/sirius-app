using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapTo(typeof(HousingPaymentPlanGroup))]
    public class UpdateHousingPaymentPlanGroupDto : FullAuditedEntityDto<Guid>
    {
        public string HousingPaymentPlanGroupName { get; set; }
        public Guid DefaultToPaymentAccountId { get; set; }
    }
}