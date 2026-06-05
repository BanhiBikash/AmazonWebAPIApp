using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.TransactionContract;
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
                return Forbid("Access mismatch. Order context belongs to a different profile account.");
            }

            // If order is already fulfilled, exit cleanly (idempotency safety)
            if (existingOrder.Status == OrderStatus.Processing.ToString()  || existingOrder.Status == OrderStatus.Failed.ToString())
            {
                return Ok(new { message = "Payment verified and recorded previously." });
            }

            // 3. SECURE INTEGRITY CHECK: Verify the cryptographic signature on the server side
            try
            {
                Dictionary<string, string> attributes = new Dictionary<string, string>();

                // If you aren't using RazorPay Orders API to generate an order ID upfront on the backend, 
                // RazorPay constructs the signature data check using the payment id combined with your secret key.
                attributes.Add("razorpay_payment_id", request.RazorpayPaymentId);

                // If utilizing razorpay order profiles, uncomment this line:
                // attributes.Add("razorpay_order_id", request.RazorpayOrderId);

                attributes.Add("razorpay_signature", request.RazorpaySignature);

                // This static method throws a SignatureVerificationError if the payload was altered
                Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception)
            {
                return BadRequest("Payment verification signature verification failed. Invalid transaction source.");
            }

            // 4. Update order status and log the permanent audit trace transaction record
            try
            {
                existingOrder.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(existingOrder);

                // Build transaction history tracker mapped precisely to your DB Schema
                AmazonWeb.Core.Domain.Entities.Transaction transactionRecord = new()
                {
                    TransactionId = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    UserId = currentUserId,
                    PaymentSource = request.RazorpayPaymentId, // Store payment reference tracking ID string
                    PaymentMethod = 0, // Maps to your Card/UPI Enum tracking indicator
                    Amount = existingOrder.TotalAmount,
                    Status = 0, // 0 = Success matching your transaction state values
                    Timestamp = DateTime.UtcNow
                };

                await _transactionRepository.RegisterTransaction(transactionRecord);

                return Ok(new { message = "Fulfillment processing successfully locked and committed." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Payment confirmed at bank, but transaction auditing logs failed: {ex.Message}");
            }
        }
    }
}
