using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Sirius.Inventories.Dto;
using Sirius.PaymentAccounts;
using System.Linq.Dynamic.Core;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Abp.UI;

namespace Sirius.Inventories
{
    public class InventoryAppService :
        AsyncCrudAppService<Inventory, InventoryDto, Guid, PagedInventoryResultRequestDto,
            CreateInventoryDto, UpdateInventoryDto>, IInventoryAppService
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IRepository<Inventory, Guid> _inventoryRepository;
        private readonly IRepository<InventoryType, Guid> _inventoryTypeRepository;
        private readonly IRepository<AccountBook, Guid> _accountBookRepository;

        public InventoryAppService(IRepository<Inventory, Guid> repository, IInventoryManager inventoryManager,
            IRepository<InventoryType, Guid> inventoryTypeRepository,
            IRepository<AccountBook, Guid> accountBookRepository, IRepository<Inventory, Guid> inventoryRepository) :
            base(repository)
        {
            _inventoryManager = inventoryManager;
            _inventoryTypeRepository = inventoryTypeRepository;
            _accountBookRepository = accountBookRepository;
            _inventoryRepository = inventoryRepository;
        }

        public override async Task<InventoryDto> CreateAsync(CreateInventoryDto input)
        {
            //Check existing invetories
            var existingInventories = await _inventoryRepository.GetAll().Where(p =>
                    p.InventoryTypeId == input.InventoryTypeId &&
                    p.SerialNumber == input.SerialNumber)
                .WhereIf(input.AccountBookId.HasValue, p => p.AccountBookId.Value == input.AccountBookId.Value)
                .ToListAsync();

            if (existingInventories.Any())
            {
                throw new UserFriendlyException(
                    "Bu seri numarasına ait bir ürün bu işletme defteri kaydı için eklenmiş");
            }

            var inventoryType = await _inventoryTypeRepository.GetAsync(input.InventoryTypeId);

            if (input.AccountBookId.HasValue)
            {
                await _accountBookRepository.GetAsync(input.AccountBookId.Value);
            }

            var inventory = Inventory.Create(SequentialGuidGenerator.Instance.Create(),
                AbpSession.GetTenantId(),
                inventoryType.Id,
                input.AccountBookId,
                input.Quantity,
                input.SerialNumber,
                input.Description);

            await _inventoryManager.CreateAsync(inventory);

            return ObjectMapper.Map<InventoryDto>(inventory);
        }

        public override async Task<InventoryDto> UpdateAsync(UpdateInventoryDto input)
        {
            var existingInventory = await _inventoryManager.GetAsync(input.Id);
            var inventoryType = await _inventoryTypeRepository.GetAsync(input.InventoryTypeId);

            if (input.AccountBookId.HasValue)
            {
                await _accountBookRepository.GetAsync(input.AccountBookId.Value);
            }

            var inventory = Inventory.Update(existingInventory, inventoryType.Id, input.AccountBookId,
                input.Quantity, input.SerialNumber, input.Description);

            await _inventoryManager.UpdateAsync(inventory);
            return ObjectMapper.Map<InventoryDto>(inventory);
        }

        public override async Task DeleteAsync(EntityDto<Guid> input)
        {
            var block = await _inventoryManager.GetAsync(input.Id);
            await _inventoryManager.DeleteAsync(block);
        }
        
        public  async Task DeleteByIdAsync(Guid id)
        {
            var block = await _inventoryManager.GetAsync(id);
            await _inventoryManager.DeleteAsync(block);
        }

        public async Task<PagedResultDto<InventoryGetAllDto>> GetAllInventoriesAsync(
            PagedInventoryResultRequestDto input)
        {
            var baseQuery = from p in _inventoryRepository.GetAll()
                join it in _inventoryTypeRepository.GetAll() on p.InventoryTypeId equals it.Id
                select new
                {
                    inventory = p,
                    inventoryType = it
                };

            var inventoriesQuery = (from q in await baseQuery.ToListAsync()
                group q by new
                    {q.inventory.SerialNumber, q.inventoryType.InventoryTypeName, q.inventoryType.QuantityType}
                into g
                select new InventoryGetAllDto
                {
                    InventoryTypeName = g.Key.InventoryTypeName,
                    QuantityTypeName = g.Key.QuantityType,
                    SerialNumber = g.Key.SerialNumber,
                    Quantity = g.Sum(s => s.inventory.Quantity),
                    AccountBookIds = g.Where(p => p.inventory.AccountBookId != null)
                        .Select(p => p.inventory.AccountBookId).ToList()
                }).AsQueryable();

            var inventories = inventoriesQuery
                .OrderBy(input.Sorting ?? $"{nameof(InventoryGetAllDto.InventoryTypeName)}")
                .PageBy(input)
                .ToList();

            return new PagedResultDto<InventoryGetAllDto>(inventoriesQuery.Count(),
                ObjectMapper.Map<List<InventoryGetAllDto>>(inventories));
        }
    }
}