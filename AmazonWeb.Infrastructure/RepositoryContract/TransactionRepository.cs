using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Infrastructure.RepositoryContract
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDBContext _dbCOntext;

        public TransactionRepository(ApplicationDBContext dbContext)
        {
            _dbCOntext = dbContext;
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            await _dbCOntext.Transactions.AddAsync(transaction);
            await _dbCOntext.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId)
        {
            // Uses .Include() to eagerly load the associated OrderItems list from the database
            return await _dbCOntext.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId)
        {
            // Fetches all historical transactions for the user, sorted by date with items included
            return await _dbCOntext.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
