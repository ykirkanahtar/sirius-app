using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sirius.AppPaymentAccounts;
using Sirius.Shared.Enums;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapFrom(typeof(PaymentAccount))]
    public class PaymentAccountDto : FullAuditedEntityDto<Guid>
    {
        public string AccountName { get; set; }
        public PaymentAccountType PaymentAccountType { get; set; }
        public decimal Balance { get; set; }
        public string Description { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string Iban { get; set; }
        public bool TenantIsOwner { get; set; }
    }
}
