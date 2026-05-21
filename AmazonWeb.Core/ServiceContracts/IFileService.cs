using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts
{
    public interface IFileService
    { 
        Task<string> UploadThumbnailAsync(IFormFile file, Guid productId); 
    }
}
