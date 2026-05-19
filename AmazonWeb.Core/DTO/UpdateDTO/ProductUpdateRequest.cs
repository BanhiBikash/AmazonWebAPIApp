using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.DTO.ResponseDTO;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.UpdateDTO
{
    public class ProductUpdateRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid Id { get; set; }

        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        public int? Price { get; set; }
        public bool? InStock { get; set; }
        public int? Stock { get; set; }

        public string? Description { get; set; }

        [Url(ErrorMessage = "Invalid image URL format")]
        public string? ImageUrl { get; set; }

        public ProductCategory? Category { get; set; }
        public ProductSubCategory? SubCategory { get; set; }

        //takes existing product and update request, applies non-null fields from update request to existing product, and returns updated product
        public static ProductResponse ApplyUpdate(ProductResponse? existingProduct, ProductUpdateRequest? updateRequest)
        {
            if (existingProduct == null)
                throw new ArgumentNullException(nameof(existingProduct));

            if (updateRequest == null)
                throw new ArgumentNullException(nameof(updateRequest));

            // Only overwrite if updateRequest field is not null
            existingProduct.Name = updateRequest.Name ?? existingProduct.Name;
            existingProduct.Price = updateRequest.Price ?? existingProduct.Price;
            existingProduct.InStock = updateRequest.InStock ?? existingProduct.InStock;
            existingProduct.Stock = updateRequest.Stock ?? existingProduct.Stock;
            existingProduct.Description = updateRequest.Description ?? existingProduct.Description;
            existingProduct.ImageUrl = updateRequest.ImageUrl ?? existingProduct.ImageUrl;
            existingProduct.Category = existingProduct.Category;
            existingProduct.SubCategory = existingProduct.SubCategory;

            return existingProduct;
        }
    }
}
