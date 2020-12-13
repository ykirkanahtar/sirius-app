using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


//https://www.gencayyildiz.com/blog/azure-storage-serisi-7-azure-blob-storage-ve-asp-net-core-ile-kullanimi/
namespace Sirius.FileServices
{
    public interface IBlobService
    {
        Task<Uri> UploadAsnyc(Stream fileStream, string name, string containerName);
        Task<Stream> DownloadAsync(string fileName, string containerName);
        Task<string> MoveBetweenContainersAsync(string fileUri, string sourceContainerName, string targetContainerName);
        Task DeleteAsync(string fileName, string containerName);
        Task SetLogAsync(string text, string fileName);
        Task<List<string>> GetLogAsync(string fileName);
        List<string> GetNames(string containerNames);
    }
}