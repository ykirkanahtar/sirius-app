using System.Threading.Tasks;
using Abp.Application.Services;
using Sirius.Sessions.Dto;

namespace Sirius.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
