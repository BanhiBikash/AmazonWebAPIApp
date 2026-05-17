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

        // Check if the database connection is alive
        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _dbContext.Database.CanConnectAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is empty, can't retrieve.");

            return await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            await _dbContext.Products.AddAsync(product);
            var result = await _dbContext.SaveChangesAsync();

            if (result == 0)
                throw new InvalidOperationException("Product wasn't added.");

            var savedProduct = await _dbContext.Products.FindAsync(product.Id);
            if (savedProduct == null)
                throw new InvalidOperationException("Product not found after save.");

            return savedProduct;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var exists = await _dbContext.Products.AnyAsync(p => p.Id == product.Id);
            if (!exists)
                throw new InvalidOperationException("Product not found, can't be updated.");

            _dbContext.Entry(product).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync();

            if (result == 0)
                throw new InvalidOperationException("Product wasn't updated.");

            return product;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id is empty, can't delete.");

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return false;

            _dbContext.Products.Remove(product);
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
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search term is empty, can't search.");

            return await _dbContext.Products
                                   .Where(p => p.Name.Contains(name))
                                   .ToListAsync();
        }
    }
}
