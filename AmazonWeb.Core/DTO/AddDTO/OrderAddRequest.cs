using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class OrderAddRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        public List<OrderItemAddRequest> Items { get; set; } = new();

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(200)]
        public string ShippingAddress { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }
    }

    public class OrderItemAddRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
