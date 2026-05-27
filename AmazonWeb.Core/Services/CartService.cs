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
        public CartService(ICartRepository cartRepository,UserManager<ApplicationUser> userManager,IProductService productService)
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

            // Fetch fully populated entities from the database (.Include(Product) is handled inside the repo)
            var rawCartItems = await _cartRepository.GetCartByUserIdAsync(userId);

            if (rawCartItems != null)
            {
                // Assigning the entity collection directly to avoid unnecessary mapping complexity
                response.Items = rawCartItems.ToList();
            }

            return response;
        }

        /// <summary>
        /// Adds an item or updates its quantity in the user's cart.
        /// </summary>
        public async Task<bool> AddOrUpdateItemAsync(Guid userId, CartRequest cartRequest)
        {
            if (cartRequest == null) return false;

            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());

            //check if user exists and cart retrieval is successful before proceeding
            if (user==null)
            {
                return false; // User not found or cart retrieval failed
            }

            // Forward parameters directly down to your repository layer logic
            return await _cartRepository.UpdateQuantityAsync(
                userId,
                cartRequest.ProductId,
                cartRequest.Quantity
            );
        }

        /// <summary>
        /// Removes a specific product row from a user's cart.
        /// </summary>
        public async Task<bool> RemoveItemAsync(Guid userId, Guid productId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            ProductResponse? product = await _productService.GetProductByIdAsync(productId);

            // Ensure both user and product exist before attempting to remove the item from the cart    
            if (user == null || product == null)
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
    }
}