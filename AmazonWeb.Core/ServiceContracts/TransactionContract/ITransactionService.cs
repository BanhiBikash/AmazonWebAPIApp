using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts.TransactionContract
{
    public interface ITransactionService
    {
        Task<TransactionResponse> RegisterTransaction(TransactionRequest? transactionRequest);

        Task<TransactionResponse> GetTransaction(Guid? TransactionID);

        Task<IEnumerable<TransactionResponse>?> GetUserTransactions(Guid? UserID);
    }
}
