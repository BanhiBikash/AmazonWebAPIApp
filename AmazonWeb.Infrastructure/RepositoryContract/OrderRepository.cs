using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        public async Task<Order> AddAsync(Order order)
        {
            //if the property is null
            if (order == null)
            {
                throw new ArgumentNullException("Order is null, can't be added.");
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

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetOrdersWithinDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
