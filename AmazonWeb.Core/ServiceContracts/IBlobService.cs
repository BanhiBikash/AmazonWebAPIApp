using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts
{
    public interface IBlobService
    {
        /// <summary>
        /// Takes the file and guid id and stores the files and returns relative url
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="productID"></param>
        /// <returns>file path(string)</returns>
        Task<string> UploadThumbnailAsync(IFormFile formFile, Guid productID);
    }
}
