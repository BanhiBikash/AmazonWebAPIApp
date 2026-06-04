using AmazonWeb.Core.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.ResponseDTO
{
    public class TransactionResponse
    {
        [Key]
        public Guid TransactionId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment source detail (Account No, UPI ID, Card no or Wallet ID) is required.")]
        public string? PaymentSource { get; set; }   //Account no., UPI ID, Wallet ID etc.

        [Required(ErrorMessage = "Order ID link tracking reference is required.")]
        public Guid OrderId { get; set; }

        [Required(ErrorMessage = "Transaction order item collection list cannot be null.")]
        public List<OrderItem>? OrderItems { get; set; }

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
    }
}
