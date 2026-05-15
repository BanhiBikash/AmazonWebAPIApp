using AmazonWeb.Core.Domain.Identities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.Domain.Entities
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }   // FK to ApplicationUser

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative")]
        public int TotalAmount => Items.Sum(i => i.Quantity * i.UnitPrice);

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(200)]
        public string? ShippingAddress { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
