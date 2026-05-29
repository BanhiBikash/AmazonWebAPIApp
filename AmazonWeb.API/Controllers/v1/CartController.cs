using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt; // 🎯 Added for clear claim mapping definitions
using System.Security.Claims;
using System.Threading.Tasks;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [AllowAnonymous] // 🔓 Overrides the global Program.cs filter to let guest traffic hit this controller layer
    public class CartController : CustomControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<CartResponse?>> GetCartByUserIdAsync()
        {
            // 🎯 Fixed: Matches the 'JwtRegisteredClaimNames.Sub' claim produced by your service
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("sub");

            // 👥 Guest Mode: If no token headers are provided, don't crash, return an empty profile state baseline
            if (string.IsNullOrEmpty(userIdString))
                return Ok(null);

            Guid userId = Guid.Parse(userIdString);
            return await _cartService.GetCartByUserIdAsync(userId);
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<CartResponse>> UpdateCart(CartRequest cartRequest)
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("sub");

            // 🔒 Guard: Guests cannot write records into the remote DB tables
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("An authenticated session token is required to update database carts.");

            Guid userId = Guid.Parse(userIdString);
            var updatedCart = await _cartService.AddOrUpdateItemAsync(userId, cartRequest);

            if (updatedCart == null)
            {
                return BadRequest("Unable to update cart layout allocations.");
            }

            return Ok(updatedCart);
        }

        [HttpDelete("[Action]")]
        public async Task<ActionResult> RemoveItem(Guid productId)
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("An authenticated session token is required to modify items.");

            Guid userId = Guid.Parse(userIdString);
            var result = await _cartService.RemoveItemAsync(userId, productId);
            if (!result)
            {
                return BadRequest("Failed to remove item from cart allocation space.");
            }
            return NoContent();
        }

        [HttpDelete("[Action]")]
        public async Task<ActionResult> ClearCart()
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized("An authenticated session token is required to clear carts.");

            Guid userId = Guid.Parse(userIdString);
            var result = await _cartService.ClearCartAsync(userId);
            if (!result)
            {
                return BadRequest("Failed to purge cart collections.");
            }
            return NoContent();
        }
    }
}