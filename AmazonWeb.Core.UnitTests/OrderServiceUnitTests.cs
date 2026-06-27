using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.Models;
using AmazonWeb.Core.ServiceContracts.OrderContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.Services.OrderService;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AmazonWeb.Core.UnitTests
{
    public class OrderServiceUnitTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly IOrderService _orderService;

        public OrderServiceUnitTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _productServiceMock = new Mock<IProductService>();
            _configurationMock = new Mock<IConfiguration>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _configurationMock.Setup(c => c["JwtSettings:Issuer"]).Returns("https://api.amazonclone.com");

            _orderService = new OrderService(
                _orderRepositoryMock.Object,
                _configurationMock.Object,
                _userManagerMock.Object,
                _productServiceMock.Object
            );
        }

        #region GetOrdersByUserID
        [Fact]
        public async Task GetOrdersByUserID_EmptyUserId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.GetOrdersByUserID(Guid.Empty));
        }

        [Fact]
        public async Task GetOrdersByUserID_ValidUser_ReturnsOrderResponses()
        {
            var userId = Guid.NewGuid();
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), UserId = userId, Status = OrderStatus.Pending }
            };

            _orderRepositoryMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(orders);

            var result = await _orderService.GetOrdersByUserID(userId);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result![0].Status.Should().Be(OrderStatus.Pending.ToString());
        }
        #endregion

        #region GetOrdersByOrderID
        [Fact]
        public async Task GetOrdersByOrderID_EmptyOrderId_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.GetOrdersByOrderID(Guid.Empty));
        }

        [Fact]
        public async Task GetOrdersByOrderID_ValidOrder_ReturnsOrderResponse()
        {
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, UserId = Guid.NewGuid(), Status = OrderStatus.Pending };

            _orderRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(order);

            var result = await _orderService.GetOrdersByOrderID(orderId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(orderId);
        }
        #endregion

        #region ReceiveOrder
        [Fact]
        public async Task ReceiveOrder_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.ReceiveOrder(null!));
        }

        [Fact]
        public async Task ReceiveOrder_RequestWithNullProperty_ThrowsArgumentNullException()
        {
            var request = new OrderAddRequest { UserId = Guid.NewGuid(), Items = null! };

            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.ReceiveOrder(request));
        }

        [Fact]
        public async Task ReceiveOrder_UserNotFound_ThrowsArgumentException()
        {
            var request = new OrderAddRequest
            {
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), ProductName = "Test", UnitPrice = 100, Quantity = 1 }
                },
                ShippingAddress = "123 Street",
                PostalCode = "12345",
                City = "City",
                Country = "Country"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.ReceiveOrder(request));
        }

        [Fact]
        public async Task ReceiveOrder_InvalidItemData_ThrowsInvalidOperationException()
        {
            var request = new OrderAddRequest
            {
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), ProductName = "Test", UnitPrice = 100, Quantity = 1 }
                },
                ShippingAddress = "123 Street",
                PostalCode = "12345",
                City = "City",
                Country = "Country"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<Guid>().ToString())).ReturnsAsync(new ApplicationUser { Id = request.UserId });
            _productServiceMock.Setup(p => p.CheckItemDataSanctity(It.IsAny<List<ItemData>>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.ReceiveOrder(request));
        }

        [Fact]
        public async Task ReceiveOrder_ValidRequest_ReturnsOrderResponse()
        {

            var request = new OrderAddRequest
            {
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), ProductName = "Test", UnitPrice = 100, Quantity = 1 }
                },
                ShippingAddress = "123 Street",
                PostalCode = "12345",
                City = "City",
                Country = "Country"
            };

            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { Id = request.UserId });
            _productServiceMock.Setup(p => p.CheckItemDataSanctity(It.IsAny<List<ItemData>>())).ReturnsAsync(true);

            var order = request.ToOrderEntity();
            _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Order>())).ReturnsAsync(order);

            var result = await _orderService.ReceiveOrder(request);

            result.Should().NotBeNull();
            result!.Status.Should().Be(OrderStatus.Pending.ToString());
        }
        #endregion

        #region UpdateOrder
        [Fact]
        public async Task UpdateOrder_NullRequest_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.UpdateOrder(null!));
        }

        //[Fact]
        //public async Task UpdateOrder_RequestWithNullProperty_ThrowsArgumentNullException()
        //{
        //    var request = new OrderUpdateRequest { Id = Guid.NewGuid(), ShippingAddress = null! };

        //    await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.UpdateOrder(request));
        //}

        //[Fact]
        //public async Task UpdateOrder_OrderNotFound_ThrowsException()
        //{
        //    var request = new OrderUpdateRequest
        //    {
        //        Id = Guid.NewGuid(),
        //        ShippingAddress = "123 Street",
        //        PostalCode = "12345",
        //        City = "City",
        //        Country = "Country",
        //        Status = OrderStatus.Pending.ToString()
        //    };

        //    _orderRepositoryMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync((Order?)null);

        //    await Assert.ThrowsAsync<Exception>(() => _orderService.UpdateOrder(request));
        //}

        //[Fact]
        //public async Task UpdateOrder_InvalidStatus_ThrowsInvalidOperationException()
        //{
        //    var request = new OrderUpdateRequest
        //    {
        //        Id = Guid.NewGuid(),
        //        ShippingAddress = "123 Street",
        //        PostalCode = "12345",
        //        City = "City",
        //        Country = "Country",
        //        Status = OrderStatus.Completed.ToString()
        //    };

        //    var order = new Order { Id = request.Id, Status = OrderStatus.Completed };
        //    _orderRepositoryMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync(order);

        //    await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.UpdateOrder(request));
        //}

        //[Fact]
        //public async Task UpdateOrder_ValidRequest_ReturnsUpdatedOrderResponse()
        //{
        //    var request = new OrderUpdateRequest
        //    {
        //        Id = Guid.NewGuid(),
        //        ShippingAddress = "123 Street",
        //        PostalCode = "12345",
        //        City = "City",
        //        Country = "Country",
        //        Status = OrderStatus.Pending.ToString()
        //    };

        //    var order = new Order { Id = request.Id, UserId = Guid.NewGuid(), Status = OrderStatus.Pending };
        //    _orderRepositoryMock.Setup(r => r.GetByIdAsync(request.Id)).ReturnsAsync(order);
        //    _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(order);

        //    var result = await _orderService.UpdateOrder(request);

        //    result.Should().NotBeNull();
        //    result!.Id.Should().Be(request.Id);
        //}
        #endregion
    }
}
