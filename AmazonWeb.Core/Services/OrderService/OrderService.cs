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
            if (userID == Guid.Empty)
            {
                throw new ArgumentException("The user id is empty");
            }

            IEnumerable<Order>? orders = await _orderRepository.GetByUserIdAsync(userID);

            // Guard against null results from the repository safely
            if (orders is null)
            {
                return new List<OrderResponse>();
            }

            // Pull the base URL out of your appsettings.json configuration file
            string? baseUrl = _configuration.GetValue<string>("JwtSettings:Issuer");
            List<OrderResponse> orderResponses = new List<OrderResponse>();

            foreach (var order in orders)
            {
                // 1. Transform the database entity into a response DTO
                OrderResponse response = order.ToOrderResponse();

                // 2. Safely loop through and prepend the base URL to every item in this specific order
                if (response.Items != null && !string.IsNullOrEmpty(baseUrl))
                {
                    response.Items.ForEach(item =>
                    {
                        if (!item.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                        {
                            item.ImageUrl = baseUrl.TrimEnd('/') + "/" + item.ImageUrl.TrimStart('/');
                        }
                    });
                }

                // 3. Add the fully mapped response to our final tracking list
                orderResponses.Add(response);
            }

            return orderResponses;
        }

        public async Task<OrderResponse?> GetOrdersByOrderID(Guid orderID)
        {
            if (orderID == Guid.Empty)
            {
                throw new ArgumentException("The order id is empty");
            }

            // 🎯 FIX 1: Query the database EXACTLY ONCE
            Order? order = await _orderRepository.GetByIdAsync(orderID);

            // 🎯 FIX 2: Check for null BEFORE executing extension methods or loops
            if (order is null)
            {
                return null; // Safely returns null to the controller (which maps to 404 Not Found)
            }

            // Transform the domain entity to our response format
            OrderResponse orderResponse = order.ToOrderResponse();

            // Pull the base URL out of your appsettings.json configuration file
            string? baseUrl = _configuration.GetValue<string>("JwtSettings:Issuer");

            // 🎯 FIX 3: Safely prepend the URL to your DTO item structures
            if (orderResponse.Items != null && !string.IsNullOrEmpty(baseUrl))
            {
                orderResponse.Items.ForEach(item =>
                {
                    // Only prepend if it's not already a fully qualified URL
                    if (!item.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        item.ImageUrl = baseUrl.TrimEnd('/') + "/" + item.ImageUrl.TrimStart('/');
                    }
                });
            }

            // Return the mutated object containing the full image paths
            return orderResponse;
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
