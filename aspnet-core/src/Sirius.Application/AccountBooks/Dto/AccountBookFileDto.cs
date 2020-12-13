using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.AccountBooks.Dto
{
    [AutoMapFrom(typeof(AccountBookFile))]
    public class AccountBookFileDto : FullAuditedEntityDto<Guid>
    {
        public string FileUrl { get; set; }

        public virtual Guid AccountBookId { get; set; }
    }
}