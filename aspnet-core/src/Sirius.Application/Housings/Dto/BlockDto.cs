using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.Housings.Dto
{
    [AutoMapFrom(typeof(Block))]
    public class BlockDto : FullAuditedEntityDto<Guid>
    {
        public string BlockName { get; set; }
    }
}