using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Sirius.Shared.Enums;

namespace Sirius.Inventories.Dto
{
    public class UpdateInventoryTypeDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        [StringLength(50)] public string InventoryTypeName { get; set; }
        [Required] public QuantityType QuantityType { get; set; }
    }
}