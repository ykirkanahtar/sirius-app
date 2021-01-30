using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Sirius.Shared.Constants;

namespace Sirius.FileServices
{
    public class UploadAppService : ApplicationService, IUploadAppService
    {
        private readonly IBlobService _blobService;
        public UploadAppService(IBlobService blobService)
        {
            _blobService = blobService;
        }
        
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null)
            {
                throw new UserFriendlyException("Dosya seçilmedi");
            }
            
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new UserFriendlyException("Geçersiz dosya");
            }
            
            var fileNameWithoutExtension = Guid.NewGuid();
            var newFileName = $"{fileNameWithoutExtension}{extension}";
            
            var result = await _blobService.UploadAsnyc(
                file.OpenReadStream(),
                newFileName,
                AppConstants.TempContainerName);
            
            return Path.Combine(result.AbsoluteUri, newFileName);
        }
    }
}