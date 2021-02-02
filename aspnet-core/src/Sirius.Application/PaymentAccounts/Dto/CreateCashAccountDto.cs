using System;
using Abp.AutoMapper;

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
        public bool IsDefault { get; set; }
        public DateTime? FirstTransferDateTime { get; set; }
        public decimal? TransferAmount { get; set; }
        public bool AllowNegativeBalance { get; set; }


        public void Normalize()
        {
            if (TransferAmount.HasValue)
            {
                TransferAmount = Math.Abs(TransferAmount.Value);
            }
        }        
    }
}
