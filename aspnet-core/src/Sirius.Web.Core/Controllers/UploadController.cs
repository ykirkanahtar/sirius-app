using System;
using System.IO;
using System.Threading.Tasks;
using Abp.Runtime.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sirius.FileServices;
using Sirius.Shared.Constants;

namespace Sirius.Controllers
{
    [DisableValidation]
    public class UploadController : SiriusControllerBase
    {
        private readonly IBlobService _blobService;

        public UploadController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpPost]
        public async Task<ActionResult> UploadAsync(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return BadRequest();
            }
            
            var fileNameWithoutExtension = Guid.NewGuid();
            var newFileName = $"{fileNameWithoutExtension}{extension}";
            
            var result = await _blobService.UploadAsnyc(
                file.OpenReadStream(),
                newFileName,
                AppConstants.TempContainerName);

            var toReturn = Path.Combine(result.AbsoluteUri, newFileName);

            return new JsonResult(toReturn);
        }
    }
}