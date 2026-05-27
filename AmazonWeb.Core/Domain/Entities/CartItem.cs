using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AmazonWeb.Core.Domain.Identities;

namespace AmazonWeb.Core.Domain.Entities
{
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; } // Foreign Key to AspNetUsers table

        [Required]
        public Guid ProductId { get; set; } // Foreign Key to Products table

        [Required]
        [Range(1, 100, ErrorMessage = "Item quantity allocation parameters must be restricted between 1 and 100 units.")]
        public int Quantity { get; set; }

        [Required]
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // ==========================================
        // Dynamic In-Memory Computations (Not Saved in Database)
        // ==========================================

        // 🎯 Safely reads the live currency price directly from your catalog product properties
        [NotMapped]
        public int UnitPrice => Product != null ? Product.Price : 0;

        // 🎯 Safely calculates dynamic row totals instantly using the live price mapping
        [NotMapped]
        public int TotalPrice => Quantity * UnitPrice;

        // ==========================================
        // Navigation Properties
        // ==========================================

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;
        public string? ProductName { get; set; }
        public string ImageUrl { get; internal set; }
    }
}