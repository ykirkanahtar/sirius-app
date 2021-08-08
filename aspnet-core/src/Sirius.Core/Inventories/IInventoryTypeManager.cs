using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Inventories
{
    public interface IInventoryTypeManager : IDomainService
    {
        Task<InventoryType> GetAsync(Guid id);
        Task CreateAsync(InventoryType inventoryType);
        Task UpdateAsync(InventoryType inventoryType);
        Task DeleteAsync(InventoryType inventoryType);
    }
}