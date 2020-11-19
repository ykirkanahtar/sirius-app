using System;
using Abp.AutoMapper;
using Sirius.AppPaymentAccounts;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(PaymentAccount))]
    public class CreateCashAccountDto
    {
        public string AccountName { get; set; }
        public string Description { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? EmployeeId { get; set; }
        public bool TenantIsOwner { get; set; }
    }
}
