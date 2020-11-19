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
        private readonly IRepository<Housing, Guid> _housingRepository;

        public BlockManager(IRepository<Block, Guid> blockRepository, IRepository<Housing, Guid> housingRepository)
        {
            _blockRepository = blockRepository;
            _housingRepository = housingRepository;
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
            var housings = await _housingRepository.GetAllListAsync(p => p.BlockId == block.Id);
            if (housings.Count > 0)
            {
                if (housings.Count == 1)
                {
                    throw new UserFriendlyException($"Bu blok/apartmana ait {housings[0].Apartment} nolu konut tanımlıdır. Silmek için önce tanımı kaldırınız.");
                }
                else
                {
                    throw new UserFriendlyException("Bu blok/apartman için birden fazla konut tanımlıdır. Silmek için önce tanımları kaldırınız.");
                }
            }

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