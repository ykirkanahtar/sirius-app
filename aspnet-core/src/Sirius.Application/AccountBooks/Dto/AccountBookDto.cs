using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Housings.Dto;
using Sirius.PaymentAccounts.Dto;
using Sirius.PaymentCategories.Dto;

namespace Sirius.AccountBooks.Dto
{
    [AutoMapFrom(typeof(AccountBook))]
    public class AccountBookDto : FullAuditedEntityDto<Guid>
    {
        public DateTime ProcessDateTime { get; set; }
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
       
        public virtual List<AccountBookFileDto> AccountBookFiles { get; set; }
        public virtual PaymentCategoryDto PaymentCategory { get; set; }
        public virtual HousingDto Housing { get; set; }
        public virtual PaymentAccountDto FromPaymentAccount { get; set; }
        public virtual PaymentAccountDto ToPaymentAccount { get; set; }
    }
}