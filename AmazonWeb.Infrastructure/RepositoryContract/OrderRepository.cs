using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
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
            var order = await _dbContext.Orders.FindAsync(id);
            if (order == null)
                return false;

            _dbContext.Orders.Remove(order);
            var result = await _dbContext.SaveChangesAsync();

            return result > 0;  //if result is 1 or more, it means the delete operation was successful
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            //egaer return so that items are not left behind, otherwise it will return orders without items
            return await _dbContext.Orders.Include(order=>order.Items).ToListAsync();
        }

        public Task<Order?> GetByIdAsync(Guid id)
        {
            if (id==Guid.Empty)
            {
                throw new ArgumentNullException("Id is empty, can't be retrieved.");
            }

            return _dbContext.Orders.Include(order => order.Items).FirstOrDefaultAsync(order => order.Id == id);
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
        {
            return await _dbContext.Orders.Include(o=>o.Items).Where(order => order.Status == status).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
        {
            if(userId == Guid.Empty)
            {
                throw new ArgumentNullException("UserId is null, can't be retrieved.");
            }

            return await _dbContext.Orders.Include(order => order.Items).Where(order => order.UserId == userId).ToListAsync();
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            Order? orderDB = await _dbContext.Orders.Where(o=>o.Id==order.Id).FirstOrDefaultAsync();

            if(orderDB == null)
            {
                throw new InvalidOperationException("Order not found, can't be updated.");
            }

            _dbContext.Entry(order).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync();

            return result>0? orderDB : null;
        }
    }
}
