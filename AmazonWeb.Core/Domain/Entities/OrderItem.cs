using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Pkcs;

namespace AmazonWeb.Core.Domain.Entities
{
    public class OrderItem
    {
        [Required]
        public Guid OrderId { get; set; }   // FK to Order

        [Required]
        public Guid ProductId { get; set; } // FK to Product

        [Required]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string ProductName { get; set; } = string.Empty; // Snapshot of product name

        [Required]
        [Url(ErrorMessage = "Invalid image URL format")]
        public string ImageUrl { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Orders are allowed in the range of 1-100")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Unit price must be non-negative")]
        public int UnitPrice { get; set; }

        [Required]
        public Order Order { get; set; }
    }
}
