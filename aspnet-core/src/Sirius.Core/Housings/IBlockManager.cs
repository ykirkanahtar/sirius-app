using System;
using System.Threading.Tasks;
using Abp.Domain.Services;

namespace Sirius.Housings
{
    public interface IBlockManager : IDomainService
    {
        Task<Block> GetAsync(Guid id);
        Task CreateAsync(Block block);
        Task UpdateAsync(Block block);
        Task DeleteAsync(Block block);
    }
}