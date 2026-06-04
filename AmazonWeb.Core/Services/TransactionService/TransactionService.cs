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

            if (transactionRequest.OrderItems == null || !transactionRequest.OrderItems.Any())
                throw new ArgumentException("Transaction must contain at least one order item.", nameof(transactionRequest));

            //check if the products in the order items are valid and available in stock and the price correct
            foreach (var item in transactionRequest.OrderItems)
            {
                // Fetch the genuine product from the database
                ProductResponse productResponse = await _productService.GetProductByIdAsync(item.ProductId);

                // Fail validation if the requested product ID does not exist in the database catalog
                if (productResponse == null)
                {
                    throw new InvalidOperationException($"Product with ID {item.ProductId} does not exist in our catalog.");
                }

                // Check 1: Verify if there is adequate stock left for the checkout item
                if (productResponse.Stock < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{productResponse.Name}'. Requested: {item.Quantity}, Available: {productResponse.Stock}."
                    );
                }

                // Check 2: Verify if the requested item price matches the live database value securely
                if (productResponse.Price != item.UnitPrice)
                {
                    throw new InvalidOperationException(
                        $"Price discrepancy detected for product '{productResponse.Name}'. The correct live price is {productResponse.Price} INR, but the request sent {item.UnitPrice} INR."
                    );
                }
            }

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
