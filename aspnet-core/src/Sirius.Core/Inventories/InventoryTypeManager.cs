using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;

namespace Sirius.Inventories
{
    public class InventoryTypeManager : IInventoryTypeManager
    {
        private readonly IRepository<InventoryType, Guid> _inventoryTypeRepository;
        private readonly IRepository<Inventory, Guid> _inventoryRepository;

        public InventoryTypeManager(IRepository<InventoryType, Guid> inventoryTypeRepository,
            IRepository<Inventory, Guid> inventoryRepository)
        {
            _inventoryTypeRepository = inventoryTypeRepository;
            _inventoryRepository = inventoryRepository;
        }


        public async Task CreateAsync(InventoryType inventoryType)
        {
            await _inventoryTypeRepository.InsertAsync(inventoryType);
        }

        public async Task UpdateAsync(InventoryType inventoryType)
        {
            await _inventoryTypeRepository.UpdateAsync(inventoryType);
        }

        public async Task DeleteAsync(InventoryType inventoryType)
        {
            var inventories = await _inventoryRepository.GetAll().Where(p => p.InventoryTypeId == inventoryType.Id)
                .CountAsync();
            if (inventories > 0)
            {
                throw new UserFriendlyException("Bu demirbaş türüne ait demirbaşlar bulunmaktadır.");
            }
            
            await _inventoryTypeRepository.DeleteAsync(inventoryType);
        }

        public async Task<InventoryType> GetAsync(Guid id)
        {
            var inventoryType = await _inventoryTypeRepository.GetAsync(id);
            if (inventoryType == null)
            {
                throw new UserFriendlyException("Demirbaş türü bulunamadı");
            }

            return inventoryType;
        }
    }
}