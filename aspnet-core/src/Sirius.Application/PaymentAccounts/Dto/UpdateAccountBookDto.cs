using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Sirius.Inventories.Dto;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class UpdateAccountBookDto : FullAuditedEntityDto<Guid>, IShouldNormalize
    {
        public UpdateAccountBookDto()
        {
            NewAccountBookFileUrls = new List<string>();
            DeletedAccountBookFileUrls = new List<string>();
            Inventories = new List<CreateInventoryDto>();
        }
        
        public string ProcessDateString { get; set; }
        public Guid PaymentCategoryId { get; set; }
        public Guid? FromPaymentAccountId { get; set; }

        public Guid? ToPaymentAccountId { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string DocumentDateTimeString { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> NewAccountBookFileUrls { get; set; }
        public List<string> DeletedAccountBookFileUrls { get; set; }
        public List<CreateInventoryDto> Inventories { get; set; }

        public void Normalize()
        {
            Amount = Math.Abs(Amount);
        }
    }
}