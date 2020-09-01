using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories.Dto
{
    [AutoMapFrom(typeof(PaymentCategory))]
    public class PaymentCategoryDto : FullAuditedEntityDto<Guid>
    {
        public string PaymentCategoryName { get; set; }
        public HousingDueType? HousingDueType { get; set; }
        
        public bool IsValidForAllPeriods { get; set; }
    }
}
