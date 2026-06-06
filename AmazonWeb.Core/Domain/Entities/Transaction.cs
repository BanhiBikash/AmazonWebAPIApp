using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmazonWeb.Core.Domain.Entities
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment source detail (Account No, UPI ID, Card no or Wallet ID) is required.")]
        public string? PaymentSource { get; set; }   //Account no., UPI ID, Wallet ID etc.

        [Required(ErrorMessage = "Order ID link tracking reference is required.")]
        public Guid OrderId { get; set; }

        // 🎯 Added: Tracks Razorpay's auto-generated Order ID (e.g., "order_OlsK83kd9s")
        [StringLength(100, ErrorMessage = "Merchant Order ID cannot exceed 100 characters.")]
        public string? PaymentMerchantOrderId { get; set; }

        // 🎯 Added: Tracks Razorpay's permanent Payment ID transaction reference (e.g., "pay_Nks938skd")
        [StringLength(100, ErrorMessage = "Merchant Transaction ID cannot exceed 100 characters.")]
        public string? PaymentMerchantTransactionId { get; set; }

        [Required(ErrorMessage = "Transaction order item collection list cannot be null.")]
        public List<OrderItem>? OrderItems { get; set; }

        [Required(ErrorMessage = "Total transaction payment amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
        public int TotalAmount { get; set; } // Represented as integer for your architecture (INR)

        [Required(ErrorMessage = "Transaction execution date and time is required.")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Transaction processing status is required.")]
        public TransactionStatus Status { get; set; } // Success, Pending, Failed, Refunded

        [Required(ErrorMessage = "Shipping physical destination address is required.")]
        [StringLength(500, ErrorMessage = "Shipping Address cannot exceed 500 characters.")]
        public string ShippingAddress { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser? User { get; set; }

        public TransactionResponse ToTransactionResponse()
        {
            return new TransactionResponse()
            {
                TransactionId = this.TransactionId,
                UserId = this.UserId,
                PaymentMethod = this.PaymentMethod,
                PaymentSource = this.PaymentSource,
                OrderId = this.OrderId,
                PaymentMerchantOrderId = this.PaymentMerchantOrderId,
                PaymentMerchantTransactionId = this.PaymentMerchantTransactionId,
                OrderItems = this.OrderItems,
                TotalAmount = this.TotalAmount,
                TransactionDate = this.TransactionDate,
                Status = this.Status,
                ShippingAddress = this.ShippingAddress
            };
        }
    }

    public enum TransactionStatus
    {
        Success,
        Pending,
        Failed,
        Refunded
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        NetBanking,
        UPI,
        Wallet
    }
}