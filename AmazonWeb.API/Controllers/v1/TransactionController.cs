using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.TransactionContract;
using AmazonWeb.Infrastructure.Migrations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Security.Claims;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    public class TransactionController : CustomControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITransactionService _transactionService;

        TransactionController(IOrderService orderService, ITransactionService transactionService)
        {
            _orderService = orderService;
            _transactionService = transactionService;
        }

        [HttpPost("[Action]")]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }
            Guid currentUserId = Guid.Parse(userIdClaim);

            // 1. Fetch the existing pending order record from your database
            var existingOrder = await _orderService.GetOrdersByOrderID(request.OrderId);
            if (existingOrder == null)
            {
                return NotFound("Order record could not be located.");
            }

            // 2. Security Guard Clause: Ensure this order belongs to the user signed into the token
            if (existingOrder.UserId != currentUserId)
            {
                // Using explicit 403 status code to prevent routing failures in JWT middleware challenge states
                return StatusCode(403, "Access mismatch. Order context belongs to a different profile account.");
            }

            // If order is already fulfilled, exit cleanly (idempotency safety)
            if (string.Equals(existingOrder.Status, OrderStatus.Processing.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(existingOrder.Status, OrderStatus.Failed.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = "Payment verified and recorded previously." });
            }

            // 3. SECURE INTEGRITY CHECK: Verify the cryptographic signature on the server side
            try
            {
                // 🎯 FIX: Razorpay signature verification explicitly requires the order ID attribute
                // even if generated completely client-side. The payload generated checks: order_id + "|" + payment_id
                Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", request.RazorpayOrderId ?? string.Empty },
            { "razorpay_payment_id", request.RazorpayPaymentId },
            { "razorpay_signature", request.RazorpaySignature }
        };

                // This static method throws a SignatureVerificationError if the payload was altered
                // Ensure you have initialized your Razorpay Client somewhere with your Key Secret, 
                // or configure it to access your environment credentials properly.
                Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {
                // Log the internal message internally for your diagnostic review
                System.Diagnostics.Debug.WriteLine($"Razorpay Signature Failure: {ex.Message}");
                return BadRequest("Payment verification signature verification failed. Invalid transaction source.");
            }

            // 4. Update order status and log the permanent audit trace transaction record
            try
            {
                OrderUpdateRequest updateRequest = new()
                {
                    Id = request.OrderId,
                    Status = OrderStatus.Processing // Update to processing as payment is confirmed
                };

                OrderResponse? updatedOrder = await _orderService.UpdateOrder(updateRequest);

                if (updatedOrder == null)
                {
                    return StatusCode(500, "Payment confirmed at bank, but failed to update order status in database.");
                }

                // Build transaction history tracker mapped precisely to your DB Schema
                TransactionRequest transactionRecord = new TransactionRequest()
                {
                    OrderId = request.OrderId,
                    UserId = currentUserId,
                    PaymentSource = request.RazorpayPaymentId, // Store payment reference tracking ID string
                    PaymentMethod = 0, // Maps to your Card/UPI Enum tracking indicator
                    TotalAmount = existingOrder.TotalAmount,
                    Status = 0, // 0 = Success matching your transaction state values
                    TransactionDate = DateTime.UtcNow
                };

                TransactionResponse? transactionResponse = await _transactionService.RegisterTransaction(transactionRecord);

                if (transactionResponse == null)
                {
                    return StatusCode(500, "Payment confirmed at bank, but failed to log transaction record in database.");
                }

                return Ok(new { message = "Fulfillment processing successfully locked and committed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Payment confirmed at bank, but transaction auditing logs failed: {ex.Message}");
            }
        }
    }
}
