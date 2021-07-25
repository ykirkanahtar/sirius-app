using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Sirius.Inventories
{
    [Table("AppInventories")]
    public class Inventory : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        [Required] public Guid InventoryTypeId { get; private set; }
        public Guid? AccountBookId { get; private set; }
        public decimal Quantity { get; private set; }
        [StringLength(50)] public string SerialNumber { get; private set; }
        [StringLength(500)] public string Description { get; private set; }

        [ForeignKey(nameof(InventoryTypeId))] public virtual InventoryType InventoryType { get; set; }

        public static Inventory Create(Guid id, int tenantId, Guid inventoryTypeId,
            Guid? accountBookId, decimal quantity, string serialNumber, string description)
        {
            return BindEntity(new Inventory(), id, tenantId,
                inventoryTypeId, accountBookId, quantity, description, serialNumber);
        }

        public static Inventory Update(Inventory existingInventor, Guid inventoryTypeId,
            Guid? accountBookId, decimal quantity, string serialNumber, string description)
        {
            return BindEntity(existingInventor, existingInventor.Id, existingInventor.TenantId,
                inventoryTypeId, accountBookId, quantity, description, serialNumber);
        }

        public static Inventory BindEntity(Inventory inventory, Guid id, int tenantId, Guid inventoryTypeId,
            Guid? accountBookId, decimal quantity, string description, string serialNumber)
        {
            inventory ??= new Inventory();
            inventory.Id = id;
            inventory.TenantId = tenantId;
            inventory.InventoryTypeId = inventoryTypeId;
            inventory.AccountBookId = accountBookId;
            inventory.Quantity = quantity;
            inventory.SerialNumber = serialNumber;
            inventory.Description = description;

            return inventory;
        }
    }
}