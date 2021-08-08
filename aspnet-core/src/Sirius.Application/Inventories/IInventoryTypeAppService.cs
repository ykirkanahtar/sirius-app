using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Inventories.Dto;
using Sirius.Shared.Dtos;

namespace Sirius.Inventories
{
    public interface IInventoryTypeAppService : IAsyncCrudAppService<InventoryTypeDto, Guid,
        PagedInventoryTypeResultRequestDto, CreateInventoryTypeDto, UpdateInventoryTypeDto>
    {
        Task<List<LookUpDto>> GetLookUp();
    }
}