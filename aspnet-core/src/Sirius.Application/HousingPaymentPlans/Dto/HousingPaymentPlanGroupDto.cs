using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Housings.Dto;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlanGroup))]
    public class HousingPaymentPlanGroupDto : FullAuditedEntityDto<Guid>
    {
        public HousingPaymentPlanGroupDto()
        {
        }

        public string HousingPaymentPlanGroupName { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public decimal AmountPerMonth { get; set; }
        public int CountOfMonth { get; set; }
        public int PaymentDayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Description { get; set; }
        public ResidentOrOwner ResidentOrOwner { get; set; }
        public string HousingCategoryNames { get; set; }

        public virtual HousingCategoryDto HousingCategory { get; set; }

        public virtual PaymentCategoryDto PaymentCategory { get; set; }
        public virtual ICollection<HousingPaymentPlanGroupHousingCategoryDto> HousingPaymentPlanGroupHousingCategories
        {
            get;
            set;
        }
    }
}