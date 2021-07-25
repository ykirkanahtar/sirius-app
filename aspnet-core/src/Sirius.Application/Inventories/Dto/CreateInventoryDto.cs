using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace Sirius.Inventories.Dto
{
    [AutoMapTo(typeof(Inventory))]
    public class CreateInventoryDto
    {
        [Required] public Guid InventoryTypeId { get; set; }
        public Guid? AccountBookId { get; set; }
        public decimal Quantity { get; set; }
        [StringLength(50)] public string SerialNumber { get; set; }
        [StringLength(500)] public string Description { get; set; }
    }
}