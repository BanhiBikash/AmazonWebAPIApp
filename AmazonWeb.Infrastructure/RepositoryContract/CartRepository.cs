using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonWeb.Infrastructure.RepositoryContract
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDBContext _context;

        // DIP: Injecting the centralized DBContext abstraction securely
        public CartRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Validates that the underlying SQL server connectivity is active and responding.
        /// </summary>
        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _context.Database.CanConnectAsync();
        }

        /// <summary>
        /// Retrieves all cart items for a specific user, eager-loading product details 
        /// so React has the prices, names, and images instantly.
        /// </summary>
        public async Task<IEnumerable<CartItem>> GetCartByUserIdAsync(Guid userId)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            return await _context.CartItems
                .Include(ci => ci.Product) // 🎯 Crucial: Loads Product data (Name, Price, Image) for React
                .Where(ci => ci.UserId == userId)
                .OrderByDescending(ci => ci.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Natively Upserts (Updates or Inserts) an item quantity parameter safely.
        /// </summary>
        public async Task<IEnumerable<CartItem>> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
        {
            // 🎯 Single DB connection check for the entire consolidated transaction sequence
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to update cart.");
            }

            if (quantity <= 0)
            {
                var targetItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
                if (targetItem != null)
                {
                    _context.CartItems.Remove(targetItem);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity = quantity;
                    existingItem.DateAdded = DateTime.UtcNow;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        DateAdded = DateTime.UtcNow
                    };
                    await _context.CartItems.AddAsync(newItem);
                }

                await _context.SaveChangesAsync();
            }

            // 🎯 REUSE Connection: Return the fresh state immediately from the exact same method
            return await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .OrderByDescending(ci => ci.DateAdded)
                .ToListAsync();
        }

        /// <summary>
        /// Removes a singular product item completely from a specific user's cart.
        /// </summary>
        public async Task<bool> RemoveItemAsync(Guid userId, Guid productId)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to remove item from cart.");
            }

            var targetItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (targetItem == null)
                return false; // Record already absent

            _context.CartItems.Remove(targetItem);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Purges all active item records assigned to a target profile GUID context.
        /// </summary>
        public async Task<bool> ClearCartAsync(Guid userId)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to clear shopping cart.");
            }

            var userItems = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!userItems.Any())
                return true; // Cart is already empty

            _context.CartItems.RemoveRange(userItems);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}