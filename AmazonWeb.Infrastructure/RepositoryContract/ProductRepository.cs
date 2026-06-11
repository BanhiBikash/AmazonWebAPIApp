using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonWeb.Infrastructure.RepositoryContract
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDBContext _dbContext;

        public ProductRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _dbContext.Database.CanConnectAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            // Simple retrieval. If it's not found, returns null. 
            // The Service Layer will handle the null check.
            return await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            return product; // EF Core automatically populates generated fields (like tracking or timestamps) directly onto the object
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            // 1. Check if EF Core is already holding a tracked instance of this product in memory
            var localInstance = _dbContext.Products.Local
                .FirstOrDefault(p => p.Id == product.Id);

            if (localInstance != null)
            {
                // 2. Detach it so it yields control of that 'Id' slot
                _dbContext.Entry(localInstance).State = EntityState.Detached;
            }

            // 3. Track the newly mapped product parameter as modified safely
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return product;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // Fast deletion: Create a stub entity with the ID so we don't have to query the database first.
            var productStub = new Product { Id = id };
            _dbContext.Products.Remove(productStub);

            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbContext.Products
                                   .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category)
        {
            return await _dbContext.Products
                                   .Where(p => p.Category == category)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetBySubCategoryAsync(ProductSubCategory subCategory)
        {
            return await _dbContext.Products
                                   .Where(p => p.SubCategory == subCategory)
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            // If name is empty, we just return an empty collection rather than crashing the app
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<Product>();

            return await _dbContext.Products
                                   .Where(p => p.Name.Contains(name) || p.Category.ToString().Contains(name) || p.SubCategory.ToString().Contains(name))
                                   .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFirstProductEachCategory()
        {
            // 1. Fetch only the Guid primary keys of the first product in each category group
            var productIds = await _dbContext.Products
                .Where(p => !p.IsDeleted) // Safely skip soft-deleted items first
                .GroupBy(p => p.Category)
                .Select(g => g.Select(p => p.Id).FirstOrDefault()) // Extracts just the Guid
                .ToListAsync();

            // 2. Hydrate the full entity payloads from the extracted primary keys
            return await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
        }
    }
}