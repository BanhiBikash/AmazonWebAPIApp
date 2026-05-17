using AmazonWeb.Core.Domain.Entities;

namespace AmazonWeb.Core.Domain.RepositoryContract
{
    /// <summary>
    /// Contract for Order repository following SOLID principles.
    /// </summary>
    public interface IOrderRepository
    {
        // SRP: Each method has a single responsibility
        // ISP: Only order-specific operations are exposed

        Task<bool> IsDatabaseAliveAsync();
        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllAsync();

        Task<Order> AddAsync(Order order);
        Task<Order> UpdateAsync(Order order);
        Task<bool> DeleteAsync(Guid id);

        // Domain-specific queries
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);

        // DIP: Services depend on this abstraction, not EF Core
    }
}
