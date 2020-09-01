using System;
using Abp.AutoMapper;
using Sirius.AppPaymentAccounts;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(PaymentAccount))]
    public class CreateBankOrAdvanceAccountDto
    {
        public string AccountName { get; set; }
        public string Description { get; set; }
        public Guid? PersonId { get; set; }
        public string Iban { get; set; }
        public bool TenantIsOwner { get; set; }
    }
}
