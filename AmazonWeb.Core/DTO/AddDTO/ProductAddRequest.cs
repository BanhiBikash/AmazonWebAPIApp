using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
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

        [Required]
        public bool InStock { get; set; }

        [Required(ErrorMessage ="Enter the quantity")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid image URL format")]
        public string ImageUrl { get; set; }

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
                InStock = request.InStock,
                Stock = request.Stock,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                Category = request.Category,
                SubCategory = request.SubCategory
            };
        }
    }
}
