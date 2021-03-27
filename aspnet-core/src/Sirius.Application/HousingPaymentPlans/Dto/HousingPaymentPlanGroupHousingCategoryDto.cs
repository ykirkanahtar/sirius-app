using System;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlanGroupHousingCategory))]
    public class HousingPaymentPlanGroupHousingCategoryDto
    {
        public Guid Id { get; set; }
        public int TenantId { get; set; }
        public Guid HousingCategoryId { get; private set; }
        public Guid HousingPaymentPlanGroupId { get; private set; }
    }
}