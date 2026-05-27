using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    public class CartController: CustomControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService) 
        { 
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<CartResponse?> GetCartByUserIdAsync(Guid userId)
        {
            return await _cartService.GetCartByUserIdAsync(userId);
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<CartResponse>> UpdateCart(Guid userId, CartRequest cartRequest)
        {
            var updatedCart = await _cartService.AddOrUpdateItemAsync(userId, cartRequest);

            if (updatedCart == null)
            {
                return BadRequest("Unable to update cart. Verify input parameters or user context.");
            }

            return Ok(updatedCart); // Returns 200 OK along with your fresh CartResponse payload instantly!
        }
    }
}
