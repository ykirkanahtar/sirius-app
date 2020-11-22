using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.PaymentCategories.Dto;
using Sirius.Shared.Enums;

namespace Sirius.HousingPaymentPlans.Dto
{
    [AutoMapFrom(typeof(HousingPaymentPlan))]
    public class HousingPaymentPlanDto : FullAuditedEntityDto<Guid>
    {
        public Guid HousingId { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public DateTime Date { get; set; }
        public PaymentPlanType PaymentPlanType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public Guid? AccountBookId { get; set; }

        public virtual PaymentCategoryDto PaymentCategory { get; set; }
        
        //Burayı açınca Angular tarafında circular dependency oluşuyor
        // public virtual HousingPaymentPlanGroupDto HousingPaymentPlanGroup { get; set; }
    }
}