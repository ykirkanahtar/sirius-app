using System;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;

namespace Sirius.Inventories
{
    public class InventoryManager : IInventoryManager
    {
        private readonly IRepository<Inventory, Guid> _inventoryRepository;

        public InventoryManager(IRepository<Inventory, Guid> inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }


        public async Task CreateAsync(Inventory inventory)
        {
            await _inventoryRepository.InsertAsync(inventory);
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            await _inventoryRepository.UpdateAsync(inventory);
        }

        public async Task DeleteAsync(Inventory inventory)
        {
            await _inventoryRepository.DeleteAsync(inventory);
        }

        public async Task<Inventory> GetAsync(Guid id)
        {
            var inventory = await _inventoryRepository.GetAsync(id);
            if (inventory == null)
            {
                throw new UserFriendlyException("Demirbaş bulunamadı");
            }

            return inventory;
        }
    }
}