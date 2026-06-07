using AmazonWeb.API.Models;
using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.ServiceContracts.TransactionContract;
using Asp.Versioning;
using Azure;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
        public async Task<ActionResult<List<OrderResponse>?>> GetOrderByUserID()
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
        public async Task<ActionResult<OrderResponse?>> GetOrdersByOrderID(Guid orderID)
        {
            if(orderID == Guid.Empty)
            {
                return BadRequest("Order ID is empty");
            }

            OrderResponse? orderResponse = await _orderService.GetOrdersByOrderID(orderID);

            return Ok(orderResponse);
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<OrderResponse>> ReceiveOrder ([FromBody] OrderAddRequest orderAddRequest)
        {
            //Extract the UserId securely from the authenticated token claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User context could not be identified from token.");
            }

            //store the data in add request
            orderAddRequest.UserId = Guid.Parse(userIdClaim);

            OrderResponse? response = await _orderService.ReceiveOrder(orderAddRequest);

            if (response == null)
            {
                return BadRequest("Unable to initialize order processing. Please verify stock availability.");
            }

            //Wrap the response model cleanly inside an Ok (200 Status) block
            return Ok(response);
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<OrderResponse?>> UpdateOrder([FromBody] OrderUpdateRequest orderUpdateRequest)
        {
            OrderResponse? updatedOrder = await _orderService.UpdateOrder(orderUpdateRequest);
            return updatedOrder;
        }
    }
}
