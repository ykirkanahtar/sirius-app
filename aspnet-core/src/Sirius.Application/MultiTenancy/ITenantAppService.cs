using Abp.Application.Services;
using Sirius.MultiTenancy.Dto;

namespace Sirius.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto,
        CreateTenantDto, TenantDto>
    {
    }
}