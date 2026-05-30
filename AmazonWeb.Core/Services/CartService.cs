using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using Microsoft.AspNetCore.Identity;

namespace AmazonWeb.Core.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductService _productService;

        // Dependency Injection to pull in your repository layer
        public CartService(ICartRepository cartRepository, UserManager<ApplicationUser> userManager, IProductService productService)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
            _productService = productService;
        }

        /// <summary>
        /// Retrieves the processed cart details for a specific user.
        /// Assigns the raw entity list straight to the response.
        /// </summary>
        public async Task<CartResponse?> GetCartByUserIdAsync(Guid userId)
        {
            var response = new CartResponse();

            // Fetch cart items from the database (.Include(Product) is handled inside the repo)
            var rawCartItems = await _cartRepository.GetCartByUserIdAsync(userId);

            if (rawCartItems != null)
            {
                foreach (CartItem item in rawCartItems)
                {
                    // Map the raw entity to a DTO for the response
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        response = await MapCartItemsToResponseAsync(rawCartItems); // Synchronously wait for the mapping to complete
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Adds an item or updates its quantity in the user's cart.
        /// </summary>
        public async Task<CartResponse?> AddOrUpdateItemAsync(Guid userId, CartRequest cartRequest)
        {
            if (cartRequest == null) return null;

            // 🔍 Single user validation check
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            // The repo executes the logic and returns the raw collection in one trip
            var updatedItems = await _cartRepository.UpdateQuantityAsync(
                userId,
                cartRequest.ProductId,
                cartRequest.Quantity
            );

            if (updatedItems == null) return null;

            //React instantly receives names, prices, images, and total calculations.
            return await MapCartItemsToResponseAsync(updatedItems);
        }

        /// <summary>
        /// Removes a specific product row from a user's cart.
        /// </summary>
        public async Task<bool> RemoveItemAsync(Guid userId, Guid productId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());

            // Ensure both user and product exist before attempting to remove the item from the cart    
            if (user == null)
            {
                return false; // User not found
            }

            return await _cartRepository.RemoveItemAsync(userId, productId);
        }

        /// <summary>
        /// Completely clears out a user's active shopping cart rows.
        /// </summary>
        public async Task<bool> ClearCartAsync(Guid userId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return false; // User not found
            }

            return await _cartRepository.ClearCartAsync(userId);
        }

        /// <summary>
        /// Shared high-performance private mapping engine to convert raw entities to DTOs.
        /// </summary>
        private async Task<CartResponse> MapCartItemsToResponseAsync(IEnumerable<CartItem> cartItems)
        {
            var response = new CartResponse();

            foreach (CartItem item in cartItems)
            {
                // 🚀 OPTIMIZATION 1: If your repository loaded the Product using .Include(),
                // we extract the data right here out of memory instantly. Zero DB overhead.
                if (item.Product != null)
                {
                    response.Items.Add(new CartItemDto
                    {
                        ProductId = item.ProductId,
                        Name = item.Product.Name,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        imageUrl = item.Product.ImageUrl,
                    });
                }
                else
                {
                    // 🛡️ FALLBACK: If the repository forgot to load the Product via an eager join, 
                    // we safely query it using your catalog service so the app doesn't crash.
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        response.Items.Add(new CartItemDto
                        {
                            ProductId = item.ProductId,
                            Name = product.Name,
                            Quantity = item.Quantity,
                            Price = product.Price,
                            imageUrl = product.ImageUrl,
                        });
                    }
                }
            }

            return response;
        }
    }
}