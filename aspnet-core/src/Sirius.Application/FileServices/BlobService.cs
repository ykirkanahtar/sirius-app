using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Sirius.FileServices
{
    public class BlobService : IBlobService
    {
        private readonly IImageService _imageService;
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient, IImageService imageService)
        {
            _blobServiceClient = blobServiceClient;
            _imageService = imageService;
        }

        private BlobContainerClient GetContainerClient(string blobContainerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            return containerClient;
        }

        public async Task<string> MoveBetweenContainersAsync(string fileUri, string sourceContainerName, string targetContainerName)
        {
            var sourceContainerClient = GetContainerClient(sourceContainerName);
            var targetContainerClient = GetContainerClient(targetContainerName);

            var fileName = Path.GetFileName(fileUri);
            var targetFileUri = $"{targetContainerName}/{fileName}";

            var sourceBlobClient = sourceContainerClient.GetBlobClient(fileName);
            var targetBlobClient = targetContainerClient.GetBlockBlobClient(fileName); 

            var blobUri = new Uri(fileUri);
            await targetBlobClient.StartCopyFromUriAsync(blobUri);
            
            await sourceBlobClient.DeleteAsync();

            return targetBlobClient.Uri.ToString();
        }
        
        public async Task<Uri> UploadAsnyc(Stream fileStream, string name, string containerName)
        {
            try
            {
                var outputStream = new MemoryStream();
                _imageService.ResizeImage(fileStream, outputStream);
            
                var blobContainerClient = GetContainerClient(containerName);
                //Container yoksa oluşturacak.
                await blobContainerClient.CreateIfNotExistsAsync();
                //Artık url üzerinden erişime açıldı.
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

                var blobClient = blobContainerClient.GetBlobClient(name);
                outputStream.Position = 0;
                await blobClient.UploadAsync(outputStream);
                return blobContainerClient.Uri;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<Stream> DownloadAsync(string fileName, string containerName)
        {
            var blobContainerClient = GetContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            //response.Value -> Tüm bilgileri getirecektir.
            return response.Value.Content;
        }
        
        public async Task DeleteAsync(string fileName, string containerName)
        {
            var blobContainerClient = GetContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            await blobClient.DeleteAsync();
        }

        public async Task<List<string>> GetLogAsync(string fileName)
        {
            var logs = new List<string>();
            var blobContainerClient = GetContainerClient("logs");//Log olacağı için ismi sabit.

            var appendBlobClient =
                blobContainerClient
                    .GetAppendBlobClient(fileName); //Üzerine eklenebilir olacağından doayı AppendBlobClient seçildi.
            await appendBlobClient.CreateIfNotExistsAsync();
            var response = await appendBlobClient.DownloadAsync();
            using var sr = new StreamReader(response.Value.Content);
            string line = string.Empty;
            while ((line = sr.ReadLine()) != null)
                logs.Add(line);
            return logs;
        }

        public List<string> GetNames(string containerNames)
        {
            var blobNames = new List<string>();
            var blobContainerClient = GetContainerClient(containerNames);
            return blobContainerClient.GetBlobs().Select(b => b.Name).ToList();
        }

        public async Task SetLogAsync(string text, string fileName)
        {
            var blobContainerClient = GetContainerClient("logs");//Log olacağı için ismi sabit.
            await blobContainerClient.CreateIfNotExistsAsync();
            var appendBlobClient = blobContainerClient.GetAppendBlobClient(fileName);
            await appendBlobClient.CreateIfNotExistsAsync();
            using MemoryStream ms = new MemoryStream();
            using StreamWriter sw = new StreamWriter(ms);
            await sw.WriteLineAsync($"{DateTime.Now} | {text}");
            await sw.FlushAsync();
            ms.Position = 0; //Stream 0. akıştan itibaren yazmaya başlayacak.
            await appendBlobClient.AppendBlockAsync(ms);
        }
    }
}