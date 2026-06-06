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
            // Validate request contract upfront
            if (request == null || request.OrderId == Guid.Empty)
            {
                return BadRequest("Invalid transaction payload. Order reference is missing.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return BadRequest("User context contains an invalid identity signature.");
            }

            // 1. Fetch the existing pending order record from your database
            var existingOrder = await _orderService.GetOrdersByOrderID(request.OrderId);
            if (existingOrder == null)
            {
                return NotFound($"Order record '{request.OrderId}' could not be located.");
            }

            // 2. Security Guard Clause: Ensure this order belongs to the user signed into the token
            if (existingOrder.UserId != currentUserId)
            {
                return StatusCode(403, "Access mismatch. Order context belongs to a different profile account.");
            }

            // If order is already fulfilled, exit cleanly (idempotency safety)
            if (string.Equals(existingOrder.Status, OrderStatus.Processing.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(existingOrder.Status, OrderStatus.Failed.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new { message = "Payment verified and recorded previously.", orderId = request.OrderId });
            }

            // 3. SECURE INTEGRITY CHECK: Verify the cryptographic signature on the server side
            try
            {
                Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "razorpay_order_id", request.RazorpayOrderId ?? string.Empty },
            { "razorpay_payment_id", request.RazorpayPaymentId },
            { "razorpay_signature", request.RazorpaySignature }
        };

                // Static method throws a SignatureVerificationError if the payload was altered.
                // Requires RazorpayClient to be configured globally in Program.cs
                Utils.verifyPaymentSignature(attributes);
            }
            catch (Exception ex)
            {
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

                // 🎯 Parse your frontend string to your custom backend PaymentMethod Enum safely
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var parsedMethod))
                {
                    parsedMethod = PaymentMethod.UPI; // Default fallback safety
                }

                // Build transaction history tracker mapped precisely to your updated Schema
                TransactionRequest transactionRecord = new TransactionRequest()
                {
                    OrderId = request.OrderId,
                    UserId = currentUserId,
                    PaymentSource = request.RazorpayPaymentId,

                    // 🎯 FIXED: Dynamic payment method tracking instead of hardcoded 0
                    PaymentMethod = parsedMethod,

                    // 🎯 FIXED: Mapping out your new merchant tracking parameters securely
                    PaymentMerchantOrderId = request.RazorpayOrderId,
                    PaymentMerchantTransactionId = request.RazorpayPaymentId,

                    TotalAmount = existingOrder.TotalAmount,
                    Status = TransactionStatus.Success, // Uses your custom enum type value 
                    TransactionDate = DateTime.UtcNow
                };

                TransactionResponse? transactionResponse = await _transactionService.RegisterTransaction(transactionRecord);

                if (transactionResponse == null)
                {
                    // 🎯 DATA INTEGRITY ROLLBACK: Revert order back if transaction logs crash
                    OrderUpdateRequest rollbackRequest = new()
                    {
                        Id = request.OrderId,
                        Status = OrderStatus.Pending
                    };
                    await _orderService.UpdateOrder(rollbackRequest);

                    return StatusCode(500, "Payment confirmed at bank, but failed to log transaction record in database. Status rolled back.");
                }

                return Ok(new { message = "Fulfillment processing successfully locked and committed.", orderId = request.OrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Payment confirmed at bank, but transaction auditing logs failed: {ex.Message}");
            }
        }

        //this is demo for bypassing the razorpay signature verification for local development testing purposes only. Do not use in production or public staging environments.
        [HttpPost("[Action]")]
        public async Task<IActionResult> ConfirmPaymentDemo([FromBody] PaymentConfirmationRequest request)
        {
            // Validate request contract upfront
            if (request == null || request.OrderId == Guid.Empty)
            {
                return BadRequest("Invalid demo transaction payload.");
            }

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
                return NotFound("Demo mode error: Order record could not be found.");
            }

            // 2. Security validation check
            if (existingOrder.UserId != currentUserId)
            {
                return StatusCode(403, "Access mismatch profile account context exception.");
            }

            // 3. 🎯 SKIP RAZORPAY SIGNATURE CRYPTO MATH ENTIRELY
            // No signature check here to keep it clean and fast for local development tests.
            System.Diagnostics.Debug.WriteLine($"[DEMO SECTOR ROUTE] Bypassing secure validation math for mock payment ID: {request.RazorpayPaymentId}");

            // 4. Update order status and write logs to your transaction tables
            try
            {
                OrderUpdateRequest updateRequest = new()
                {
                    Id = request.OrderId,
                    Status = OrderStatus.Processing // Flags order layout as paid
                };

                OrderResponse? updatedOrder = await _orderService.UpdateOrder(updateRequest);
                if (updatedOrder == null)
                {
                    return StatusCode(500, "Demo failed updating order status flags.");
                }

                // Parse your simulated incoming string type back to the enum context
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var parsedMethod))
                {
                    parsedMethod = PaymentMethod.Wallet; // Default demo choice
                }

                // Build mock history database instance values
                TransactionRequest transactionRecord = new TransactionRequest()
                {
                    OrderId = request.OrderId,
                    UserId = currentUserId,
                    PaymentSource = "DEMO_BYPASS_MODE_SOURCE",
                    PaymentMethod = parsedMethod,

                    // Log the mock identifier tokens generated on your frontend
                    PaymentMerchantOrderId = request.RazorpayOrderId,
                    PaymentMerchantTransactionId = request.RazorpayPaymentId,

                    TotalAmount = existingOrder.TotalAmount,
                    Status = TransactionStatus.Success,
                    TransactionDate = DateTime.UtcNow
                };

                TransactionResponse? transactionResponse = await _transactionService.RegisterTransaction(transactionRecord);

                if (transactionResponse == null)
                {
                    return StatusCode(500, "Demo failed writing transaction record schema entry.");
                }

                return Ok(new { message = "Demo Bypass transaction committed and logged cleanly.", orderId = request.OrderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Demo internal process failure layout mapping error: {ex.Message}");
            }
        }
    }
}
