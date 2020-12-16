using System;
using Abp.AutoMapper;
using Abp.Runtime.Validation;

namespace Sirius.AccountBooks.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class CreateTransferForPaymentAccountDto : IShouldNormalize
    {
        public DateTime ProcessDateTime { get; set; }
        public decimal Amount { get; set; }


        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}