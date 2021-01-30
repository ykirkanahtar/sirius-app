using System;
using System.Collections.Generic;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateHousingDueAccountBookDto : IShouldNormalize
    {
        public CreateHousingDueAccountBookDto()
        {
            AccountBookFileUrls = new List<string>();
        }
        public DateTime ProcessDateTime { get; set; }
        public Guid HousingId { get; set; }
        public Guid ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public List<string> AccountBookFileUrls { get; set; }
        
        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}
