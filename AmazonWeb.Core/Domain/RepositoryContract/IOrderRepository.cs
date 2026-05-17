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

        Task<Order?> GetByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetAllAsync();

        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Guid id);

        // Domain-specific queries
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetOrdersWithinDateRangeAsync(DateTime startDate, DateTime endDate);

        // DIP: Services depend on this abstraction, not EF Core
    }
}
