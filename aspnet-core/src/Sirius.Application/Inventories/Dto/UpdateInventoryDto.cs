using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Sirius.Inventories.Dto
{
    public class UpdateInventoryDto : IEntityDto<Guid>
    {
        public Guid Id { get; set; }
        [Required] public Guid InventoryTypeId { get; set; }
        public Guid? AccountBookId { get; set; }
        public decimal Quantity { get; set; }
        [StringLength(50)] public string SerialNumber { get; set; }
        [StringLength(500)] public string Description { get; set; }
    }
}