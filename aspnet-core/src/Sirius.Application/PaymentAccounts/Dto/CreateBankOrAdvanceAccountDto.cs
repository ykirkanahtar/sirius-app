using System;
using Abp.AutoMapper;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(PaymentAccount))]
    public class CreateBankOrAdvanceAccountDto
    {
        public string AccountName { get; set; }
        public string Description { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Iban { get; set; }
        public bool TenantIsOwner { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? FirstTransferDateTime { get; set; }
        public decimal? TransferAmount { get; set; }


        public void Normalize()
        {
            if (TransferAmount.HasValue)
            {
                TransferAmount = Math.Abs(TransferAmount.Value);
            }
        }  
    }
}
