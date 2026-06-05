using AmazonWeb.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class OrderAddRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;

        [Required(ErrorMessage = "At least one item is required")]
        public List<OrderItem> Items { get; set; }

        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(200)]
        public string? ShippingAddress { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public Order ToOrderEntity()
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = ShippingAddress,
                PostalCode = PostalCode,
                City = City,
                Country = Country,
                Items = Items
            };
        }
    }
}
