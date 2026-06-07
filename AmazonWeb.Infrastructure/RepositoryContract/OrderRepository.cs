using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;

namespace AmazonWeb.Infrastructure.RepositoryContract
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDBContext _dbContext;

        public OrderRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        //check if the database connection is alive
        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _dbContext.Database.CanConnectAsync();
        }// 🎯 IMPLEMENT THIS: Safely wipe the local change tracker memory
        
        public void ClearTracker()
        {
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<Order> AddAsync(Order order)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            await _dbContext.Orders.AddAsync(order);
            int result = await _dbContext.SaveChangesAsync();

            if (result == 0)
                throw new InvalidOperationException("Order wasn't placed");

            var savedOrder = await _dbContext.Orders.FindAsync(order.Id);
            if (savedOrder == null)
                throw new InvalidOperationException("Order not found after save.");

            return savedOrder;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null)
                return false;

            _dbContext.Orders.Remove(order);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;  //if result is 1 or more, it means the delete operation was successful
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            //egaer return so that items are not left behind, otherwise it will return orders without items
            return await _dbContext.Orders.Include(order=>order.Items).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            if (id==Guid.Empty)
            {
                throw new ArgumentNullException("Id is empty, can't be retrieved.");
            }

            return await _dbContext.Orders.Include(order => order.Items).AsNoTracking().FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            return await _dbContext.Orders.Include(o=>o.Items).Where(order => order.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentNullException("UserId is empty, can't be retrieved.");
            }

            return await _dbContext.Orders.Include(order => order.Items).Where(order => order.UserId == userId).ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            if (!await IsDatabaseAliveAsync())
            {
                throw new InvalidOperationException("Database connectivity check failed. Unable to fetch cart data.");
            }

            Order? orderToUpdate = await _dbContext.Orders.FindAsync(order.Id);

            // 🎯 THE FIX: Stop execution and return null safely if the database doesn't have this record
            if (orderToUpdate == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            orderToUpdate.ShippingAddress = order.ShippingAddress;
            orderToUpdate.PostalCode = order.PostalCode;
            orderToUpdate.City = order.City;
            orderToUpdate.Country = order.Country;
            orderToUpdate.Status = order.Status;

            //save the data
            await _dbContext.SaveChangesAsync();

            return orderToUpdate;
        }
    }
}
