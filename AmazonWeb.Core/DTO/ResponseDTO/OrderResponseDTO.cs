using AmazonWeb.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmazonWeb.Core.DTO.ResponseDTO
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; }

        // 🎯 Derived from the items collection total sum matching your Domain Entity logic
        public int TotalAmount => Items.Sum(i => i.UnitPrice*i.Quantity);

        // 🎯 Converts your backend OrderStatus enum cleanly to its string name for your React views
        public string Status { get; set; } = string.Empty;

        // Shipping Details
        public string? ShippingAddress { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        // 🎯 Flat DTO list containing only frontend visibility properties
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}