using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.IO;
using webapi.Service;

namespace webapi.Container
{
    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        public StorageService(
            BlobServiceClient blobServiceClient, 
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        public async Task<byte[]> GetFileBlobAsync(string fileName)
        {
            var containerName = _configuration.GetValue<string>("ContainerName");
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();

                using (MemoryStream stream = new MemoryStream())
                {
                    response.Value.Content.CopyTo(stream);

                    return stream.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public async Task UploadFile(IFormFile formfile)
        {
            
                        var containerName = _configuration.GetValue<string>("ContainerName");
                        await _blobServiceClient.GetBlobContainerClient(containerName).CreateIfNotExistsAsync();           
                        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                        using var stream = new MemoryStream();

                        await formfile.CopyToAsync(stream);
                        stream.Position = 0;
                        await containerClient.UploadBlobAsync(formfile.FileName, stream);
                                   
        }

        public async Task UploadMultiFile(IList<IFormFile> files)
        {

            var containerName = _configuration.GetValue<string>("ContainerName")+Guid.NewGuid();
            await _blobServiceClient.GetBlobContainerClient(containerName).CreateIfNotExistsAsync();
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            foreach (IFormFile file in files)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    await containerClient.UploadBlobAsync(file.FileName, stream);
                }
            }
        }

        public async Task<String> DeleteFromBlob(string fileName)
        {

            var containerName = _configuration.GetValue<string>("ContainerName");
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
  
            var blobClient = containerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return "File Deleted";
            }
            else
            {
                return "No files to Delete";
            }
                 
        }

    }
}
