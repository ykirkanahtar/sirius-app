using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.AppPaymentAccounts;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(PaymentAccount))]
    public class UpdatePaymentAccountDto : FullAuditedEntityDto<Guid>
    {
        public string AccountName { get; set; }
        public string Description { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Iban { get; set; }
        public bool TenantIsOwner { get; set; }
    }
}