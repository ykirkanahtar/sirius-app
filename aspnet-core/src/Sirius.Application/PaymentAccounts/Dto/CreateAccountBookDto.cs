using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Shared.Enums;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateAccountBookDto : IShouldNormalize
    {
        public CreateAccountBookDto()
        {
            AccountBookFileUrls = new List<string>();
        }
        public DateTime ProcessDateTime { get; set; }
        public PaymentCategoryType PaymentCategoryType { get; set; }
        public bool IsHousingDue { get; set; }
        public Guid HousingId { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }
        public Guid? ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> AccountBookFileUrls { get; set; }
        
        public bool EncachmentFromHousingDue { get; set; }
        
        public Guid? HousingIdForEncachment { get; set; }
        public Guid? PaymentCategoryIdForEncachment { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
