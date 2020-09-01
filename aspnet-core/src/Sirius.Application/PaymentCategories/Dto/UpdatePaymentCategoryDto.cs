using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Sirius.PaymentCategories.Dto
{
    public class UpdatePaymentCategoryDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        [StringLength(50)]
        public string PaymentCategoryName { get; set; }
        
        public bool IsValidForAllPeriods { get; set; }
    }
}
