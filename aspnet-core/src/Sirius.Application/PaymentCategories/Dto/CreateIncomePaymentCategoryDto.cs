using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.PaymentCategories.Dto
{
    [AutoMapTo(typeof(PaymentCategory))]
    public class CreateIncomePaymentCategoryDto
    {
        [StringLength(50)]
        public string PaymentCategoryName { get; set; }
        public bool IsValidForAllPeriods { get; set; }
        public Guid DefaultToPaymentAccountId { get; set; }
    }
}