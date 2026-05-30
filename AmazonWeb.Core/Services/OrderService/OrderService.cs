using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AmazonWeb.Core.Services.OrderService
{
    public class OrderService : IOrderService
    {
        //readonly vars for DI
        private readonly IOrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _user;

        //for DI
        public OrderService(IOrderRepository orderRepository, IConfiguration configuration, UserManager<ApplicationUser> user)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _user = user;
        }

        //Get orders of a particular email
        public async Task<List<OrderResponse>?> GetOrdersByUserID(Guid userID)
        {
            if(userID == Guid.Empty)
            {
                throw new ArgumentException("The user id is empty");
            }

            IEnumerable<Order>? orders = await _orderRepository.GetByUserIdAsync(userID);

            List<OrderResponse>? orderResponses = new List<OrderResponse>();

            foreach (var order in orders)
            {
                 orderResponses.Add(order.ToOrderResponse());
            }

            return orderResponses;
        }

        public async Task<OrderResponse?> GetOrdersByOrderID(Guid OrderID)
        {
            if(OrderID == Guid.Empty)
            {
                throw new ArgumentException("The order id is empty");
            }

            return await _orderRepository.GetByIdAsync(OrderID) is Order order ? order.ToOrderResponse() : null;
        }

        public async Task<OrderResponse?> ReceiveOrder(OrderAddRequest request)
        {
            //check if the request is null
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "The order request is null");
            }

            //check if any of the properties of the request is null
            foreach (var property in typeof(OrderAddRequest).GetProperties())
            {
                if (property.GetValue(request) == null)
                {
                    throw new ArgumentNullException(property.Name, $"The {property.Name} property is null");   
                }
            }

            ApplicationUser? user =await _user.FindByIdAsync(request.UserId.ToString());

            if(user == null)
            {
                throw new ArgumentException("Please register to put in an order.Error at Order Service.");
            }

            Order? orderResponse = await _orderRepository.AddAsync(request.ToOrderEntity());

            return orderResponse.ToOrderResponse();
        }
    }
}
