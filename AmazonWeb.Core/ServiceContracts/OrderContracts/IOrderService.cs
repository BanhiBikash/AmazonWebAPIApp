using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;

namespace AmazonWeb.Core.ServiceContracts.OrderContracts
{
    public interface IOrderService
    {
        /// <summary>
        /// Receives the order
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns the order response</returns>
        Task<OrderResponse?> ReceiveOrder(OrderAddRequest request);

        /// <summary>
        /// Takes the user ID
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns>returns the orders of a user</returns>
        Task<List<OrderResponse>?> GetOrdersByUserID(Guid userID);

        /// <summary>
        /// Takes the OrderID
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns>Returns the order response</returns>
        Task<OrderResponse?> GetOrdersByOrderID(Guid OrderID);
    }
}
