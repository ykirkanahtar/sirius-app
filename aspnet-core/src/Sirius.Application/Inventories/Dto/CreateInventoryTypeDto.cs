using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using Sirius.Shared.Enums;

namespace Sirius.Inventories.Dto
{
    [AutoMapTo(typeof(InventoryType))]
    public class CreateInventoryTypeDto
    {
        [StringLength(50)] [Required] public string InventoryTypeName { get; set; }
        [Required] public QuantityType QuantityType { get; set; }
    }
}