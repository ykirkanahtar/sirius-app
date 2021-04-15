using System;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlanGroupHousingCategory))]
    public class HousingPaymentPlanGroupHousingCategoryDto
    {
        public Guid Id { get; set; }
        public int TenantId { get; set; }
        public Guid HousingCategoryId { get; set; }
        public Guid HousingPaymentPlanGroupId { get; set; }
        public decimal AmountPerMonth { get; set; }
    }
}