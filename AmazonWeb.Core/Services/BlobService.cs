using AmazonWeb.Core.ServiceContracts;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.Services
{
    public class BlobService : IBlobService
    {
        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<BlobContainerClient> CreateBlobContainerClient()
        {

            //fetch blob connection string
            var connectionString = _configuration["BlobStorageConnectionString"];
            BlobContainerClient blobClient = new BlobContainerClient(connectionString,"data");
            await blobClient.CreateIfNotExistsAsync();
            return blobClient;
        }

        public async Task<string> UploadThumbnailAsync(IFormFile formFile, Guid productID)
        {
            if (formFile == null || formFile.Length == 0 || productID == Guid.Empty)
                return "resources/thumbnail/default.png";

            var fileName = "resources/thumbnail/" + productID.ToString() + Path.GetExtension(formFile.FileName);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    //create the client
                    var blobClient = await CreateBlobContainerClient();
                    //copy filedata into memory stream
                    formFile.CopyTo(memoryStream);
                    //set position 0
                    memoryStream.Position = 0;

                    //upload the blob by blob name
                    await blobClient.UploadBlobAsync(fileName, memoryStream);

                    return fileName;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"failed to upload {formFile.FileName}"+ex.Message);
                return "resources/thumbnail/default.png";
            }
        }
    }
}
