using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Authorization.Accounts.Dto;

namespace Sirius.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
