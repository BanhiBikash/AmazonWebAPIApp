using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.AddDTO; // 🎯 Assumed namespace for your Data Transfer Objects

namespace AmazonWeb.Core.ServiceContracts.CartContracts
{
    /// <summary>
    /// Service contract defining business logic operations for the Shopping Cart.
    /// DIP: Controllers will depend on this abstraction.
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Retrieves the processed cart details for a specific user, 
        /// mapped safely to a DTO for the React frontend.
        /// </summary>
        Task<CartResponse?> GetCartByUserIdAsync(Guid userId);

        /// <summary>
        /// Adds an item or updates its quantity in the user's cart, 
        /// executing business rule validations first.
        /// </summary>
        Task<CartResponse?> AddOrUpdateItemAsync(Guid userId, CartRequest cartRequest);

        /// <summary>
        /// Removes a specific product row from a user's cart.
        /// </summary>
        Task<bool> RemoveItemAsync(Guid userId, Guid productId);

        /// <summary>
        /// Completely clears out a user's active shopping cart rows.
        /// </summary>
        Task<bool> ClearCartAsync(Guid userId);

        /// <summary>
        /// Merges Local and database cart and syncs to database
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guestItems"></param>
        /// <returns>Merged-cart</returns>
        Task<CartResponse?> MergeCartAsync(Guid userId, List<CartRequest> guestItems);
    }
}