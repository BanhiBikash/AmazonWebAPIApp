using AmazonWeb.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.Services
{
    public class LocalFileService : IFileService
    {

        private readonly IConfiguration _configuration;

        public LocalFileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadThumbnailAsync(IFormFile file, Guid productId)
        {
            if (file == null || file.Length == 0)
                return "/resources/product-thumbnail/default.png"; // Fallback placeholder

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resources", "product-thumbnail");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{productId}{Path.GetExtension(file.FileName)}";
            //var filePath = Path.Combine(folderPath, fileName);

            //blob storage save
            string blobBaseUrl = _configuration["BlobBaseUrl"];
            var filePath = Path.Combine(blobBaseUrl, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/resources/product-thumbnail/{fileName}";
        }
    }
}
