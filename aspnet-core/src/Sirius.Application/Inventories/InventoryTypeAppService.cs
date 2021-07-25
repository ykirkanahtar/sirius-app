using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using Sirius.Inventories.Dto;
using Sirius.Shared.Dtos;
using System.Linq;

namespace Sirius.Inventories
{
    public class InventoryTypeAppService :
        AsyncCrudAppService<InventoryType, InventoryTypeDto, Guid, PagedInventoryTypeResultRequestDto,
            CreateInventoryTypeDto, UpdateInventoryTypeDto>, IInventoryTypeAppService
    {
        private readonly IInventoryTypeManager _inventoryTypeManager;
        private readonly IRepository<InventoryType, Guid> _inventoryTypeRepository;

        public InventoryTypeAppService(
            IRepository<InventoryType, Guid> inventoryTypeRepository,
            IInventoryTypeManager inventoryTypeManager) : base(
            inventoryTypeRepository)
        {
            _inventoryTypeManager = inventoryTypeManager;
            _inventoryTypeRepository = inventoryTypeRepository;
        }

        public override async Task<InventoryTypeDto> CreateAsync(CreateInventoryTypeDto input)
        {
            var inventoryType = InventoryType.Create(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                input.InventoryTypeName,
                input.QuantityType
            );

            await _inventoryTypeManager.CreateAsync(inventoryType);

            return ObjectMapper.Map<InventoryTypeDto>(inventoryType);
        }

        public override async Task<InventoryTypeDto> UpdateAsync(UpdateInventoryTypeDto input)
        {
            var existingInventoryType = await _inventoryTypeManager.GetAsync(input.Id);

            var inventoryType =
                InventoryType.Update(existingInventoryType, input.InventoryTypeName, input.QuantityType);

            await _inventoryTypeManager.UpdateAsync(inventoryType);
            return ObjectMapper.Map<InventoryTypeDto>(inventoryType);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var block = await _inventoryTypeManager.GetAsync(input.Id);
            await _inventoryTypeManager.DeleteAsync(block);
        }

        public async override Task<InventoryTypeDto> GetAsync(EntityDto<Guid> input)
        {
            var value =  await base.GetAsync(input);
            return value;
        }

        public override async Task<PagedResultDto<InventoryTypeDto>> GetAllAsync(
            PagedInventoryTypeResultRequestDto input)
        {
            var query = _inventoryTypeRepository.GetAll();

            var inventoryTypes = await query
                .OrderBy(input.Sorting ?? $"{nameof(InventoryType.InventoryTypeName)}")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<InventoryTypeDto>(query.Count(),
                ObjectMapper.Map<List<InventoryTypeDto>>(inventoryTypes));
        }

        public async Task<List<LookUpDto>> GetLookUp()
        {
            var inventoryTypes = await _inventoryTypeRepository.GetAllListAsync();

            return
                (from l in inventoryTypes
                    select new LookUpDto(l.Id.ToString(), l.InventoryTypeName)).ToList();
        }
    }
}