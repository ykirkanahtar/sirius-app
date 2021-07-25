using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Inventories
{
    public interface IInventoryManager : IDomainService
    {
        Task<Inventory> GetAsync(Guid id);
        Task CreateAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
        Task DeleteAsync(Inventory inventory);
    }
}