using AmazonWeb.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazonWeb.Core.Domain.RepositoryContract
{
    /// <summary>
    /// Contract for Shopping Cart repository following SOLID principles.
    /// Handles volatile, pre-checkout customer items.
    /// </summary>
    public interface ICartRepository
    {
        // 🔍 Health check validation (DIP / Infrastructure monitoring)
        Task<bool> IsDatabaseAliveAsync();

        // 🎯 Fetch all items belonging to a specific customer's cart
        Task<IEnumerable<CartItem>> GetCartByUserIdAsync(Guid userId);

        // 🎯 Dynamic alteration handling: inserts a new row or updates quantity parameters
        Task<bool> UpdateQuantityAsync(Guid userId, Guid productId, int quantity);

        // 🎯 Removes a specific product item row from a user's cart
        Task<bool> RemoveItemAsync(Guid userId, Guid productId);

        // 🎯 Clears the entire cart instantly (Crucial right after a successful checkout order completes!)
        Task<bool> ClearCartAsync(Guid userId);
    }
}