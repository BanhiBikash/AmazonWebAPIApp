using AmazonWeb.Core.DTO.AddDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts.TransactionContract
{
    public interface ITransactionService
    {
        Task<> RegisterTransaction(TransactionRequest? transactionRequest);
    }
}
