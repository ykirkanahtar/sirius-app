using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.PaymentAccounts.Dto
{
    [AutoMapTo(typeof(AccountBook))]
    public class UpdateAccountBookDto : FullAuditedEntityDto<Guid>
    {
        public string Description { get; set; }
        public DateTime? DocumentDateTime { get; set; }
        public string DocumentNumber { get; set; }
        public List<string> AccountBookFiles { get; set; }
    }
}