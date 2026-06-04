using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AmazonWeb.API.Models;
using AmazonWeb.Core.ServiceContracts.TransactionContract;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    public class OrdersController : CustomControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ITransactionService _transactionService;

        public OrdersController(IOrderService orderService, IProductService productService, ITransactionService transactionService)
        {
            _orderService = orderService;
            _productService = productService;
            _transactionService = transactionService;
        }

        [Route("[Action]")]
        [HttpGet]
        public async Task<ActionResult?> GetOrderByUserID()
        {
            //Standardized fallback chain across ALL actions to capture auto-mapped tokens
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            //check if the string is empty
            if (string.IsNullOrEmpty(userIdString)) 
            {
                return BadRequest("User is not registered or user is null");
            }

            //Order Response 
            List< OrderResponse>? orderResponse= await _orderService.GetOrdersByUserID(Guid.Parse(userIdString));

            return Ok(orderResponse);
        }


        [HttpGet("[Action]")]
        public async Task<ActionResult> GetOrdersByOrderID(Guid orderID)
        {
            if(orderID == Guid.Empty)
            {
                return BadRequest("Order ID is empty");
            }

            OrderResponse? orderResponse = await _orderService.GetOrdersByOrderID(orderID);

            return Ok(orderResponse);
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<OrderResponse>> ReceiveOrder ([FromForm] CheckoutRequest checkoutRequest)
        {
            // ==========================================================================
            // STEP 1: Candidate Verification - Structural & Payload Level
            // ==========================================================================
            if (checkoutRequest == null)
                return BadRequest("Order Request payload is null.");

            // Reflection property verification loop
            foreach (var property in typeof(CheckoutRequest).GetProperties())
            {
                if (property.Name == nameof(CheckoutRequest.Items)) continue;

                if (property.GetValue(checkoutRequest) == null)
                {
                    return BadRequest($"The property {property.Name} is required and cannot be null.");
                }
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ==========================================================================
            // STEP 2: Candidate Verification - Price Match Integrity Check and Stock Check
            // ==========================================================================
            foreach (var item in checkoutRequest.Items)
            {
                // Pure Service Access: Fetching cleaned data from your ProductService
                ProductResponse? liveProduct = await _productService.GetProductByIdAsync(item.ProductId);

                if (liveProduct == null)
                {
                    return BadRequest($"Product with ID {item.ProductId} does not exist in our system.");
                }

                if (liveProduct.Price != item.UnitPrice)
                {
                    return BadRequest($"Price validation discrepancy detected for product '{liveProduct.Name}'. The secure catalog price is {liveProduct.Price} INR, but front-end submitted {item.UnitPrice} INR.");
                }

                if (liveProduct.Stock < item.Quantity)
                {
                    return BadRequest($"Insufficient warehouse stock for product '{liveProduct.Name}'. Requested quantity: {item.Quantity}. Remaining inventory balances: {liveProduct.Stock}.");
                }
            }

            //create a orderID which will be there as common in both Order and Transaction 
            Guid sharedOrderId = Guid.NewGuid(); 

            // ==========================================================================
            // STEP 3: Execution / Inventory Operations 
            // ==========================================================================
            // If the transaction succeeded, execute stock changes purely through the ProductService layer
            if (checkoutRequest.TransactionStatus == TransactionStatus.Success)
            {
                try
                {
                    foreach (var item in checkoutRequest.Items)
                    {
                        // Safely request stock reductions via service layer mapping pipelines
                        await _productService.DeductProductStockAsync(item.ProductId, item.Quantity);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // ==========================================================================
            // STEP 5: Final Sequenced Persistence Pass with Payment Recovery Safety Net
            // ==========================================================================
            OrderResponse? orderResponse = null;

            try
            {
                // A. Attempt to map and save the Order record first
                orderResponse = await _orderService.ReceiveOrder(checkoutRequest.MapToOrderRequest(sharedOrderId));

                if (orderResponse == null)
                {
                    throw new Exception("Order service returned a null response during insertion.");
                }

                // B. If the order succeeds, log the Successful Transaction normally
                TransactionRequest? transactionRequest = checkoutRequest.MapToTransactionRequest(sharedOrderId);
                await _transactionService.RegisterTransaction(transactionRequest);

                return Ok(orderResponse);
            }
            catch (Exception orderException)
            {
                // 🎯 THE PAYMENT SAFETY NET
                // If we reach here, the order failed to save. BUT if the money was actually received,
                // we MUST force save the transaction as a Success so we don't lose track of their money!
                if (checkoutRequest.TransactionStatus == TransactionStatus.Success)
                {
                    try
                    {
                        TransactionRequest? recoveryTransactionRequest = checkoutRequest.MapToTransactionRequest(sharedOrderId);

                        // Optional but highly recommended: Append a tracking message to the source or metadata 
                        recoveryTransactionRequest.PaymentSource += " [ORPHANED_PAYMENT_SYSTEM_CRASH_RECOVERY]";

                        // Force save the successful transaction history log into the database
                        await _transactionService.RegisterTransaction(recoveryTransactionRequest);

                        // Log this critical alert to your server console or logging tool (Serilog/ILogger)
                        Console.WriteLine($"CRITICAL: Payment was captured successfully, but the order failed to save for SharedOrderId: {sharedOrderId}. Transaction history preserved.");

                        return StatusCode(500, new
                        {
                            message = "Your payment was successful, but a temporary system error occurred while generating your order profile. Please contact customer support with your transaction details.",
                            trackingReference = sharedOrderId
                        });
                    }
                    catch (Exception transactionLoggingException)
                    {
                        // Absolute worst case scenario (Database completely down): 
                        // Return an error containing the exact tracking IDs so it appears in the frontend logs or network tab
                        return StatusCode(500, $"CRITICAL FAIL: Payment captured but database is fully offline. Reference ID: {sharedOrderId}. Error: {transactionLoggingException.Message}");
                    }
                }
                else    //Store unsuccessfull order and transaction data
                {
                    // A. Attempt to map and save the Order record first
                    OrderAddRequest? failedOrder = checkoutRequest.MapToOrderRequest(sharedOrderId);
                    failedOrder.ShippingAddress = "Failed";
                    orderResponse = await _orderService.ReceiveOrder(failedOrder);

                    if(orderResponse == null)
                    {
                        throw new Exception("Failed to save failed order in database");
                    }

                    TransactionRequest? recoveryTransactionRequest = checkoutRequest.MapToTransactionRequest(sharedOrderId);

                    if(recoveryTransactionRequest == null)
                    {
                        throw new Exception("Failed to save tranaction in database");
                    }
                }

                    // If the payment was a failure anyway from the gateway, return standard error response
                    return StatusCode(500, $"An error broke checkout pipelines: {orderException.Message}");
            }

        }
    }
}
