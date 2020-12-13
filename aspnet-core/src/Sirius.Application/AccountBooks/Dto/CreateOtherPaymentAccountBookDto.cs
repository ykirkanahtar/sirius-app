using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.AccountBooks.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateOtherPaymentAccountBookDto : IShouldNormalize
    {
        public CreateOtherPaymentAccountBookDto()
        {
            AccountBookFileUrls = new List<string>();
        }
        public DateTime ProcessDateTime { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }
        public Guid? ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> AccountBookFileUrls { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
