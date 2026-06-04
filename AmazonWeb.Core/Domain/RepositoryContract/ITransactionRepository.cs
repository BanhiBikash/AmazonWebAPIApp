using AmazonWeb.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.Domain.RepositoryContract
{
    public interface ITransactionRepository
    {
        /// <summary>
        /// Saves a brand new transaction record along with its child order items into the database.
        /// </summary>
        /// <param name="transaction">The raw Transaction domain entity to be persisted.</param>
        /// <returns>The persisted Transaction entity populated with database-generated tracking context.</returns>
        Task<Transaction> AddAsync(Transaction transaction);

        /// <summary>
        /// Fetches a specific transaction by its unique TransactionId.
        /// </summary>
        /// <param name="transactionId">The Guid unique identifier of the transaction.</param>
        /// <returns>The tracking Transaction record, or null if not found.</returns>
        Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);

        /// <summary>
        /// Retrieves all transaction history records belonging to a specific customer profile.
        /// </summary>
        /// <param name="userId">The Guid user reference key matching the ApplicationUser identity registry.</param>
        /// <returns>A collection of transactions linked to the requested user profile.</returns>
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId);
    }
}
