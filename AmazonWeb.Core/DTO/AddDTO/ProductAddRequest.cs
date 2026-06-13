using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class ProductAddRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Stock status is required")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public double Discount { get; set; } = 0d;

        [Required]
        public bool InStock { get; set; }

        [Required(ErrorMessage ="Enter the quantity")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Product thumbnail is required")]
        public IFormFile Thumbnail{ get; set; }

        [Required(ErrorMessage = "Category is required")]
        public ProductCategory Category { get; set; }

        [Required(ErrorMessage = "SubCategory is required")]
        public ProductSubCategory? SubCategory { get; set; }

        //get product for add request
        public static Product ToProduct(ProductAddRequest request)
        {
            return new Product
            {
                Id = Guid.NewGuid(), // new product gets a fresh ID
                Name = request.Name,
                Price = request.Price,
                Discount = request.Discount,
                InStock = request.InStock,
                Stock = request.Stock,
                Description = request.Description,
                Category = request.Category,
                SubCategory = request.SubCategory
            };
        }
    }
}
