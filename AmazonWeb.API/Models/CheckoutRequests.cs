using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.DTO.AddDTO;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.API.Models
{
    public class CheckoutRequest
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        // ==========================================
        // 🏡 SHIPPING & DELIVERY DETAILS (Maps to Order)
        // ==========================================
        [Required(ErrorMessage = "Shipping destination address is required.")]
        [StringLength(200, ErrorMessage = "Shipping Address cannot exceed 200 characters.")]
        public string? ShippingAddress { get; set; }

        [Required(ErrorMessage = "Postal Code is required.")]
        [StringLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "City name is required.")]
        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Country name is required.")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters.")]
        public string? Country { get; set; }

        // ==========================================
        // 💳 TRANSACTION DATA (Maps to Transaction)
        // ==========================================
        [Required(ErrorMessage = "Payment method selection is required.")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment source configuration token reference is required.")]
        public string? PaymentSource { get; set; }

        [Required(ErrorMessage = "Transaction processing verdict is required.")]
        public TransactionStatus TransactionStatus { get; set; } // Success or Failed

        // ==========================================
        // 📦 NESTED PRODUCT COLLECTION BUNDLE 
        // ==========================================
        [Required(ErrorMessage = "Checkout list must contain at least one product item row.")]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();


        // ==========================================
        // 🧩 DOMAIN MAPPING ORCHESTRATION METHODS
        // ==========================================

        /// <summary>
        /// Compiles the incoming flat data graph straight into an Order Domain Entity.
        /// </summary>
        public OrderAddRequest MapToOrderRequest()
        {
            var order = new OrderAddRequest
            {
                UserId = this.UserId,
                ShippingAddress = this.ShippingAddress,
                PostalCode = this.PostalCode,
                City = this.City,
                Country = this.Country
            };

            foreach (var item in this.Items)
            {
                order.Items.Add(new OrderItem
                {
                    OrderId = Guid.Empty,   //Temporary placeholder, will be set in the OrderService when creating the Order entity
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice // Maps directly to your OrderItem configuration rules
                });
            }

            return order;
        }

        /// <summary>
        /// Compiles the incoming flat data graph straight into a Transaction Domain Entity.
        /// </summary>
        public TransactionRequest MapToTransactionRequest(Guid sharedOrderId)
        {
            var transaction = new TransactionRequest
            {
                UserId = this.UserId,
                OrderId = sharedOrderId, // Bound to our parent Order tracking key
                PaymentMethod = this.PaymentMethod,
                PaymentSource = this.PaymentSource,
                TransactionDate = DateTime.UtcNow,
                Status = this.TransactionStatus, // Success or Failed

                // Concat shipping items into a string format for your 500-char Transaction ledger configuration
                ShippingAddress = $"{this.ShippingAddress}, {this.City}, {this.PostalCode}, {this.Country}".Trim().Trim(','),

                // Map the shared OrderItem reference lists over 
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in this.Items)
            {
                transaction.OrderItems.Add(new OrderItem
                {
                    OrderId = sharedOrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            return transaction;
        }
    }
}