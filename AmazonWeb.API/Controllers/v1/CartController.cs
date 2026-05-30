using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [AllowAnonymous] // 🔓 Allows guest traffic to hit baseline mapping flows safely
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
            // 🎯 FIXED: Standardized fallback chain across ALL actions to capture auto-mapped tokens
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 👥 Guest Mode Baseline
            if (string.IsNullOrEmpty(userIdString))
                return Ok(null);

            Guid userId = Guid.Parse(userIdString);
            CartResponse? cartResponse = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cartResponse);
        }

        [HttpPost("[Action]")]
        public async Task<IActionResult> MergeCart([FromBody] List<CartRequest> guestItems)
        {
            // 1. Identify user via Token Claims
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            Guid userId = Guid.Parse(userIdString);

            // 🎯 CLEAN ARCHITECTURE: Delegate all processing and mapping to the Service
            CartResponse? updatedCart = await _cartService.MergeCartAsync(userId, guestItems);

            return Ok(updatedCart ?? new CartResponse());
        }

        [HttpPost("[Action]")]
        public async Task<ActionResult<CartResponse>> UpdateCart(CartRequest cartRequest)
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 🔒 Guard: Authenticated context required
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
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

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
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                               ?? User.FindFirstValue("sub")
                               ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

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