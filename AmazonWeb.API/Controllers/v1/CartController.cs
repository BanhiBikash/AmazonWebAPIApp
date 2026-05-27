using AmazonWeb.Core.Domain.RepositoryContract;
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

        [HttpGet]
        public async Task<> 
    }
}
