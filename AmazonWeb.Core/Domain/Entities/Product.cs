using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.DTO.ResponseDTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.Domain.Entities
{
    public class Product
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Price { get; set; }

        [Required(ErrorMessage = "There must be a discount percentage")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public double Discount { get; set; }

        [Required]
        public bool InStock { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Url(ErrorMessage = "Invalid image URL format")]
        public string ImageUrl { get; set; }

        [Required]
        public ProductCategory Category { get; set; }

        [Required]
        public ProductSubCategory? SubCategory { get; set; }

        public bool IsDeleted { get; set; } = false;

        public static ProductResponse ToProductResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price - Convert.ToInt32((product.Price * product.Discount) / 100.0),
                CatalogPrice = product.Price,
                Discount = product.Discount,
                InStock = product.InStock,
                Stock = product.Stock,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Category = product.Category.ToString(),
                SubCategory = product.SubCategory.ToString(),
                IsDeleted = product.IsDeleted
            };
        }
    }
}
