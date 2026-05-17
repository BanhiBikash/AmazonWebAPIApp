using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
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
    }
}
