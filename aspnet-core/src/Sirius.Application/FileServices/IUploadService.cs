using System.Threading.Tasks;
using Abp.Application.Services;
using Microsoft.AspNetCore.Http;

namespace Sirius.FileServices
{
    public interface IUploadAppService : IApplicationService
    {
        Task<string> UploadFileAsync(IFormFile file);
    }
}