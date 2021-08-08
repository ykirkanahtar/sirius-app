using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Sirius.Inventories.Dto;

namespace Sirius.Inventories
{
    public interface IInventoryAppService : IAsyncCrudAppService<InventoryDto, Guid,
        PagedInventoryResultRequestDto, CreateInventoryDto, UpdateInventoryDto>
    {
        Task DeleteFromInventoryPage(DeleteInventoryDto input);
        Task DeleteByIdAsync(Guid id);
        Task<PagedResultDto<InventoryGetAllDto>> GetAllInventoriesAsync(
            PagedInventoryResultRequestDto input);
    }
}