using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.People;
using Sirius.Shared.Enums;

namespace Sirius.Housings
{
    public class BlockManager : IBlockManager
    {
        private readonly IRepository<Block, Guid> _blockRepository;

        public BlockManager(IRepository<Block, Guid> blockRepository)
        {
            _blockRepository = blockRepository;
        }

        public async Task CreateAsync(Block block)
        {
            await _blockRepository.InsertAsync(block);
        }

        public async Task UpdateAsync(Block block)
        {
            await _blockRepository.UpdateAsync(block);
        }

        public async Task DeleteAsync(Block block)
        {
            await _blockRepository.DeleteAsync(block);
        }

        public async Task<Block> GetAsync(Guid id)
        {
            var block = await _blockRepository.GetAsync(id);
            if (block == null)
            {
                throw new UserFriendlyException("Blok bulunamadı");
            }

            return block;
        }
    }
}