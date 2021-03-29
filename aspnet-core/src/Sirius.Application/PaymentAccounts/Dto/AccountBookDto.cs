using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Housings.Dto;
using Sirius.PaymentCategories.Dto;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapFrom(typeof(AccountBook))]
    public class AccountBookDto : FullAuditedEntityDto<Guid>
    {
        public DateTime ProcessDateTime { get; set; }
        public AccountBookType AccountBookType { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? HousingId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }
        public Guid? ToPaymentAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
        public decimal? FromPaymentAccountCurrentBalance { get; set; }
        public decimal? ToPaymentAccountCurrentBalance { get;  set; }
        public bool NettingHousing { get;  set; }
        public Guid? HousingIdForNetting { get;  set; }
        public int SameDayIndex { get; set; }

        public virtual List<AccountBookFileDto> AccountBookFiles { get; set; }
    }
}