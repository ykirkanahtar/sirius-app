using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace Sirius.PaymentAccounts.Dto
{
    public class AccountBookGetAllOutput: FullAuditedEntityDto<Guid>
    {
        public DateTime ProcessDateTime { get; set; }
        public string PaymentCategoryName { get; set; }
        public string HousingName { get; set; }
        public decimal Amount { get; set; }
        public string FromPaymentAccountName { get; set; }
        public string ToPaymentAccountName { get; set; }
        public decimal? FromPaymentAccountBalance { get; set; }
        public decimal? ToPaymentAccountBalance { get; set; }
        public int SameDayIndex { get; set; }
        public List<string> AccountBookFiles { get; set; }
    }
}