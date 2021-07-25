using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.Inventories.Dto
{
    [AutoMapFrom(typeof(InventoryType))]
    public class InventoryTypeDto : FullAuditedEntityDto<Guid>
    {
        public string InventoryTypeName { get; set; }
        public QuantityType QuantityType { get; set; }
    }
}