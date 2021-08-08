using System;

namespace Sirius.Inventories.Dto
{
    public class DeleteInventoryDto
    {
        public Guid InventoryTypeId { get; set; }
        public decimal Quantity { get; set; }
        public string SerialNumber { get; set; }
    }
}