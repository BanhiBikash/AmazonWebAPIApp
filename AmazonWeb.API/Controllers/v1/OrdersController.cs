using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using Asp.Versioning;
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

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
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
        public async Task<ActionResult?> ReceiveOrder(OrderAddRequest addRequest)
        {
            if (addRequest == null) return BadRequest("Order Request is null");

            foreach(var property in typeof(OrderAddRequest).GetProperties())
            {
                if (property.GetValue(addRequest) == null)
                {
                    return BadRequest($"The property {property.Name} is required and cannot be null.");
                }
            }

            OrderResponse? orderResponse = await _orderService.ReceiveOrder(addRequest);

            return Ok(orderResponse);
        }
    }
}
