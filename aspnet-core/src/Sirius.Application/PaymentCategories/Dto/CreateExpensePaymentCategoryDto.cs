using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace Sirius.PaymentCategories.Dto
{
    [AutoMapTo(typeof(PaymentCategory))]
    public class CreateExpensePaymentCategoryDto
    {
        [StringLength(50)]
        public string PaymentCategoryName { get; set; }
        public bool IsValidForAllPeriods { get; set; }
        public Guid DefaultFromPaymentAccountId { get; set; }
    }
}