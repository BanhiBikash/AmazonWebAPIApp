using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.ResponseDTO
{
    public class ProductResponse
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

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        public string Category { get; set; }
        public string? SubCategory { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Parameterless constructor for serialization
        public ProductResponse() { }
    }
}
