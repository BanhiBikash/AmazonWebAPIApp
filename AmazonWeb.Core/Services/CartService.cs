using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AmazonWeb.Core.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;
        private readonly IConfiguration _configuration;

        public CartService(ICartRepository cartRepository, UserManager<ApplicationUser> userManager, IProductService productService, IConfiguration configuration)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
            _productService = productService;
            _configuration = configuration;
        }

        /// <summary>
        /// Retrieves the processed cart details for a specific user.
        /// </summary>
        public async Task<CartResponse?> GetCartByUserIdAsync(Guid userId)
        {
            var rawCartItems = await _cartRepository.GetCartByUserIdAsync(userId);
            if (rawCartItems == null) return new CartResponse();

            // 🎯 FIXED: Removed the performance draining N+1 loop completely. 
            // The mapping engine handles the translation safely in a single pass.
            return await MapCartItemsToResponseAsync(rawCartItems);
        }

        /// <summary>
        /// Adds an item or updates its quantity in the user's cart.
        /// </summary>
        public async Task<CartResponse?> AddOrUpdateItemAsync(Guid userId, CartRequest cartRequest)
        {
            if (cartRequest == null) return null;

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            var updatedItems = await _cartRepository.UpdateQuantityAsync(
                userId,
                cartRequest.ProductId,
                cartRequest.Quantity
            );

            if (updatedItems == null) return null;

            // 🎯 FIXED: No need to append anything here. Private mapper handles it cleanly.
            return await MapCartItemsToResponseAsync(updatedItems);
        }

        /// <summary>
        /// Removes a specific product row from a user's cart.
        /// </summary>
        public async Task<bool> RemoveItemAsync(Guid userId, Guid productId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            return await _cartRepository.RemoveItemAsync(userId, productId);
        }

        /// <summary>
        /// Completely clears out a user's active shopping cart rows.
        /// </summary>
        public async Task<bool> ClearCartAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return false;

            return await _cartRepository.ClearCartAsync(userId);
        }

        /// <summary>
        /// Merges a guest cookie cart into the persistent user database cart upon authentication login.
        /// </summary>
        public async Task<CartResponse?> MergeCartAsync(Guid userId, List<CartRequest> guestItems)
        {
            if (guestItems == null || guestItems.Count == 0)
            {
                return await GetCartByUserIdAsync(userId);
            }

            var currentDbCart = await _cartRepository.GetCartByUserIdAsync(userId);
            var dbItems = currentDbCart ?? new List<CartItem>();

            foreach (var item in guestItems)
            {
                if (item.ProductId == Guid.Empty || item.Quantity <= 0)
                    continue;

                var commonDbItem = dbItems.FirstOrDefault(i => i.ProductId == item.ProductId);

                int targetQuantity = item.Quantity;
                if (commonDbItem != null)
                {
                    targetQuantity = commonDbItem.Quantity + item.Quantity;
                }

                var isolatedPayload = new CartRequest
                {
                    ProductId = item.ProductId,
                    Quantity = targetQuantity
                };

                await AddOrUpdateItemAsync(userId, isolatedPayload);
            }

            // 🎯 FIXED: Clean return mapping with zero redundant text manipulation loops.
            return await GetCartByUserIdAsync(userId);
        }

        /// <summary>
        /// Single Point of Truth: Shared private mapping engine to handle all DTO formatting,
        /// configuration checks, and defensive base URL prepending routines safely in memory.
        /// </summary>
        private async Task<CartResponse> MapCartItemsToResponseAsync(IEnumerable<CartItem> cartItems)
        {
            var response = new CartResponse();
            string? baseUrl = _configuration.GetValue<string>("JwtSettings:Issuer");

            foreach (CartItem item in cartItems)
            {
                CartItemDto dtoItem;

                if (item.Product != null)
                {
                    // Memory Extraction Optimization via eager loading (.Include)
                    dtoItem = new CartItemDto
                    {
                        ProductId = item.ProductId,
                        Name = item.Product.Name,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        imageUrl = item.Product.ImageUrl ?? string.Empty
                    };
                }
                else
                {
                    // Fallback database lookup tracking if eager loading was omitted
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product == null) continue; // Skip orphan database tracking anomalies safely

                    dtoItem = new CartItemDto
                    {
                        ProductId = item.ProductId,
                        Name = product.Name,
                        Quantity = item.Quantity,
                        Price = product.Price,
                        imageUrl = product.ImageUrl ?? string.Empty
                    };
                }

                // 🎯 THE CENTRALIZED URL SANITIZER: Run your string transformations right here, exactly once.
                if (!string.IsNullOrEmpty(baseUrl) && !dtoItem.imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    dtoItem.imageUrl = baseUrl.TrimEnd('/') + "/" + dtoItem.imageUrl.TrimStart('/');
                }

                response.Items.Add(dtoItem);
            }

            return response;
        }
    }
}