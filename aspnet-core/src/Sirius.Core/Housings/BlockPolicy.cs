using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.People;

namespace Sirius.Housings
{
    public class BlockPolicy : IBlockPolicy
    {
        private readonly IRepository<Block, Guid> _blockRepository;

        public BlockPolicy(IRepository<Block, Guid> blockRepository)
        {
            _blockRepository = blockRepository;
        }

        public async Task CheckCreateOrUpdateBlockAttemptAsync(Block block, bool isUpdate)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(Block));
            }

            if (block.BlockName.IsNullOrWhiteSpace())
            {
                throw new UserFriendlyException("Blok ismi dolu olmalıdır.");
            }

            var blockWithSameNameQuery =
                _blockRepository.GetAll().Where(p =>
                        string.Equals(p.BlockName.ToLower(), block.BlockName.ToLowerInvariant()))
                    .WhereIf(isUpdate, p => p.Id != block.Id);

            if ((await blockWithSameNameQuery.ToListAsync()).Count > 0)
            {
                throw new UserFriendlyException(
                    $"{block.BlockName} daha önceden tanımlanmış.");
            }
        }
    }
}