using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;

namespace AmazonWeb.Core.Domain.RepositoryContract
{
    /// <summary>
    /// Contract for Product repository following SOLID principles.
    /// </summary>
    public interface IProductRepository
    {
        // SRP: Each method has a single responsibility
        // ISP: Only product-specific operations are exposed
        Task<bool> IsDatabaseAliveAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product> AddAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(Guid id);

        // Domain-specific queries
        Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category);
        Task<IEnumerable<Product>> GetBySubCategoryAsync(ProductSubCategory subCategory);
        Task<IEnumerable<Product>> SearchByNameAsync(string name);

        // Liskov Substitution Principle: Any implementation should respect contract
        // Dependency Inversion Principle: High-level modules depend on this abstraction, not EF Core
    }
}
