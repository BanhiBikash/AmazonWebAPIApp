using AmazonWeb.Core.ServiceContracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.Services
{
    public class LocalFileService : IFileService
    {
        public async Task<string> UploadThumbnailAsync(IFormFile file, Guid productId)
        {
            if (file == null || file.Length == 0)
                return "/resources/product-thumbnail/default.png"; // Fallback placeholder

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resources", "product-thumbnail");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{productId}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/resources/product-thumbnail/{fileName}";
        }
    }
}
