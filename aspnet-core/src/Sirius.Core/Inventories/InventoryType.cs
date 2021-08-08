using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Sirius.Shared.Enums;

namespace Sirius.Inventories
{
    [Table("AppInventoryTypes")]
    public class InventoryType : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        [StringLength(50)] [Required] public string InventoryTypeName { get; private set; }
        [Required] public QuantityType QuantityType { get; private set; }

        public static InventoryType Create(Guid id, int tenantId, string inventoryTypeName, QuantityType quantityType)
        {
            return BindEntity(new InventoryType(), id, tenantId, inventoryTypeName, quantityType);
        }

        public static InventoryType Update(InventoryType existingInventoryType, string inventoryTypeName,
            QuantityType quantityType)
        {
            return BindEntity(existingInventoryType, existingInventoryType.Id, existingInventoryType.TenantId,
                inventoryTypeName, quantityType);
        }

        public static InventoryType BindEntity(InventoryType inventoryType, Guid id, int tenantId,
            string inventoryTypeName, QuantityType quantityType)
        {
            inventoryType ??= new InventoryType();
            inventoryType.Id = id;
            inventoryType.TenantId = tenantId;
            inventoryType.InventoryTypeName = inventoryTypeName;
            inventoryType.QuantityType = quantityType;

            return inventoryType;
        }
    }
}