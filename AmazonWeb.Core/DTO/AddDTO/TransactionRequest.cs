using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class TransactionRequest

    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment source detail (Account No, UPI ID, Card no or Wallet ID) is required.")]
        public string? PaymentSource { get; set; }   //Account no., UPI ID, Wallet ID etc.

        [Required(ErrorMessage = "Order ID link tracking reference is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "Payment merchant order ID is required.")]
        public string? PaymentMerchantOrderId { get; set; }

        [Required(ErrorMessage = "Payment Trsanaction ID is also needed.")]
        public string? PaymentMerchantTransactionId { get; set; }

        [Required(ErrorMessage = "Total transaction payment amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
        public int TotalAmount { get; set; } // Represented as integer for your architecture (INR)

        [Required(ErrorMessage = "Transaction execution date and time is required.")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Transaction processing status is required.")]
        [StringLength(20, ErrorMessage = "Status value cannot exceed 20 characters.")]
        public TransactionStatus Status { get; set; } // Success, Pending, Failed, Refunded

        [Required(ErrorMessage = "Shipping physical destination address is required.")]
        [StringLength(500, ErrorMessage = "Shipping Address cannot exceed 500 characters.")]
        public string ShippingAddress { get; set; } = string.Empty;

        public Transaction ToTransaction()
        {
            return new Transaction()
            {
                TransactionId = Guid.NewGuid(),
                UserId = this.UserId,
                PaymentMethod = this.PaymentMethod,
                PaymentSource = this.PaymentSource,
                OrderId = this.OrderId,
                PaymentMerchantOrderId = this.PaymentMerchantOrderId,
                PaymentMerchantTransactionId = this.PaymentMerchantTransactionId,
                TotalAmount = this.TotalAmount,
                TransactionDate = this.TransactionDate,
                Status = this.Status,
                ShippingAddress = this.ShippingAddress
            };
        }
    }
}
