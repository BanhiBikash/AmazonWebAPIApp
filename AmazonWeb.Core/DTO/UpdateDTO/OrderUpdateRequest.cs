using AmazonWeb.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.UpdateDTO
{
    public class OrderUpdateRequest
    {
        [Required(ErrorMessage = "Order ID is required")]
        public Guid Id { get; set; }

        [StringLength(200)]
        public string? ShippingAddress { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public OrderStatus? Status { get; set; }
    }
}
