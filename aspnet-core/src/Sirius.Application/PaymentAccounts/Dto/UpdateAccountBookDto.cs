using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class UpdateAccountBookDto : FullAuditedEntityDto<Guid>, IShouldNormalize
    {
        public UpdateAccountBookDto()
        {
            NewAccountBookFileUrls = new List<string>();
            DeletedAccountBookFileUrls = new List<string>();
        }
        
        public DateTime ProcessDateTime { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }

        public Guid? ToPaymentAccountId { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> NewAccountBookFileUrls { get; set; }
        public List<string> DeletedAccountBookFileUrls { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}