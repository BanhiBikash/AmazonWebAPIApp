using AmazonWeb.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmazonWeb.Core.DTO.ResponseDTO
{
    public class CartResponse
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // 🎯 Instantly tells your React global state header navbar how many items are in the cart badge
        public int TotalItems => Items.Sum(i => i.Quantity);

        // 🎯 The grand total price calculation displayed at checkout
        public int GrandTotal => Items.Sum(i => i.TotalPrice);
    }
}