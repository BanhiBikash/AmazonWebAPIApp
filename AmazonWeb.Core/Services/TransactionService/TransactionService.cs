using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.ServiceContracts.TransactionContract;
using AmazonWeb.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;

namespace AmazonWeb.Core.Services.TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(IProductService productService, UserManager<ApplicationUser> userManager, ITransactionRepository transactionRepository)
        {
            _productService = productService;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
        }
        public async Task<TransactionResponse> GetTransaction(Guid TransactionID)
        {
            if(TransactionID == Guid.Empty)
                throw new ArgumentException("Transaction ID cannot be an empty GUID.", nameof(TransactionID));
            
            // Simulate fetching transaction details from the database
            TransactionResponse? transactionResponse = (await _transactionRepository.GetTransactionByIdAsync(TransactionID)).ToTransactionResponse();

            return transactionResponse;
        }

        public async Task<TransactionResponse> RegisterTransaction(TransactionRequest transactionRequest)
        {
            // 1. Fail early if the incoming request payload is null
            if (transactionRequest == null)
                throw new ArgumentNullException(nameof(transactionRequest));

            // If all validations pass, proceed to create the transaction record in the database
            Transaction newTransaction = transactionRequest.ToTransaction();

            //push to Database and receive response back
            TransactionResponse transactionResponse = (await _transactionRepository.AddAsync(newTransaction)).ToTransactionResponse();

            return transactionResponse;
        }

        public async Task<IEnumerable<TransactionResponse>?> GetUserTransactions(Guid? UserID)
        {
            if (UserID == null)
                throw new ArgumentNullException(nameof(UserID), "User ID cannot be null.");

            if (UserID == Guid.Empty)
                throw new ArgumentException("User ID cannot be an empty GUID.", nameof(UserID));

            //check if user exists in the database
            ApplicationUser? user = await _userManager.FindByIdAsync(UserID.ToString());

            if (user == null)
                throw new InvalidOperationException($"User with ID {UserID} does not exist.");

            //Fetch the transactions for the user from the database
            IEnumerable<TransactionResponse>? transactions = user.Transactions?.Select(t => t.ToTransactionResponse());

            return transactions;
        }
    }
}
