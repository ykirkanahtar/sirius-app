using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sirius.HousingCategories;
using Sirius.Housings.Dto;
using Sirius.People;
using Sirius.Shared.Dtos;

namespace Sirius.Housings
{
    public class BlockAppService :
        AsyncCrudAppService<Block, BlockDto, Guid, PagedBlockResultRequestDto, CreateBlockDto, UpdateBlockDto
        >, IBlockAppService
    {
        private readonly IBlockManager _blockManager;
        private readonly IRepository<Block, Guid> _blockRepository;
        private readonly IBlockPolicy _blockPolicy;

        public BlockAppService(IBlockManager blockManager, IRepository<Block, Guid> blockRepository,
            IBlockPolicy blockPolicy)
            : base(blockRepository)
        {
            _blockManager = blockManager;
            _blockRepository = blockRepository;
            _blockPolicy = blockPolicy;
        }

        public override async Task<BlockDto> CreateAsync(CreateBlockDto input)
        {
            var block = await Block.CreateAsync(_blockPolicy, SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                input.BlockName);
            await _blockManager.CreateAsync(block);
            return ObjectMapper.Map<BlockDto>(block);
        }

        public override async Task<BlockDto> UpdateAsync(UpdateBlockDto input)
        {
            var existingBlock = await _blockManager.GetAsync(input.Id);

            var block = await Block.UpdateAsync(_blockPolicy, existingBlock, input.BlockName);
            await _blockManager.UpdateAsync(block);
            return ObjectMapper.Map<BlockDto>(block);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var block = await _blockManager.GetAsync(input.Id);
            await _blockManager.DeleteAsync(block);
        }

        public override async Task<PagedResultDto<BlockDto>> GetAllAsync(PagedBlockResultRequestDto input)
        {
            var query = _blockRepository.GetAll();

            var blocks = await query.OrderBy(input.Sorting ?? $"{nameof(BlockDto.BlockName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<BlockDto>(query.Count(),
                ObjectMapper.Map<List<BlockDto>>(blocks));
        }

        public async Task<List<LookUpDto>> GetBlockLookUpAsync()
        {
            var blocks = await _blockRepository.GetAllListAsync();

            return
                (from l in blocks
                    select new LookUpDto(l.Id.ToString(), l.BlockName)).ToList();
        }
    }
}