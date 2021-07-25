using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace Sirius.Inventories.Dto
{
    [AutoMapFrom(typeof(Inventory))]
    public class InventoryDto : FullAuditedEntityDto<Guid>
    {
        public Guid InventoryTypeId { get; set; }
        public Guid? AccountBookId { get; set; }
        public decimal Quantity { get; set; }
        public string SerialNumber { get; set; }
        public string Description { get; set; }

        public virtual InventoryTypeDto InventoryType { get; set; }
    }
}