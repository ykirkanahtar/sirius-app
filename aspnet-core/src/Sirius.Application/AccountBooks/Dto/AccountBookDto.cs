using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.PaymentCategories;
using Sirius.Shared.Enums;

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
    }
}