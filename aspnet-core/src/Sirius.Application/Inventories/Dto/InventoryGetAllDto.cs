using System;
using System.Collections.Generic;
using Sirius.PaymentAccounts.Dto;
using Sirius.Shared.Enums;

namespace Sirius.Inventories.Dto
{
    public class InventoryGetAllDto
    {
        public Guid InventoryTypeId { get; set; }
        public string InventoryTypeName { get; set; }
        public string SerialNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityWithAccountBook { get; set; }
        public QuantityType QuantityTypeName { get; set; }
        public List<Guid?> AccountBookIds { get; set; }
    }
}