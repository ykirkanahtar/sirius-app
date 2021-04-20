using System;
using Abp.AutoMapper;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlanGroupHousing))]
    public class HousingPaymentPlanGroupHousingDto
    {
        public Guid Id { get; set; }
        public int TenantId { get; set; }
        public Guid HousingId { get; set; }
        public Guid HousingPaymentPlanGroupId { get; set; }
        public decimal AmountPerMonth { get; set; }
    }
}