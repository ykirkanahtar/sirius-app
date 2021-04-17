using System;
using Abp.AutoMapper;
using Sirius.Authorization.Users;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapFrom(typeof(AccountBookGetAllOutput))]
    public class AccountBookGetAllExportOutput
    {
        public string ProcessDateTime { get; set; }
        public string PaymentCategoryName { get; set; }
        public string HousingName { get; set; }
        public decimal Amount { get; set; }
        public string FromPaymentAccountName { get; set; }
        public string ToPaymentAccountName { get; set; }
        public decimal? FromPaymentAccountBalance { get; set; }
        public decimal? ToPaymentAccountBalance { get; set; }
    }
}