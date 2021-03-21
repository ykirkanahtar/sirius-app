using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.PaymentAccounts.Dto;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories.Dto
{
    [AutoMapFrom(typeof(PaymentCategory))]
    public class PaymentCategoryDto : FullAuditedEntityDto<Guid>
    {
        public string PaymentCategoryName { get; set; }
        public bool IsHousingDue { get; set; }
        public bool IsValidForAllPeriods { get; set; }
        
        public Guid? DefaultFromPaymentAccountId { get; set; }
        public Guid? DefaultToPaymentAccountId { get; set; }
        
        public PaymentCategoryType PaymentCategoryType { get; set; }
        
        public string DefaultFromPaymentAccountName { get; set; }
        public string DefaultToPaymentAccountName { get; set; }
    }
}
