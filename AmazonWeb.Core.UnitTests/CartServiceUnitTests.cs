using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.ServiceContracts.CartContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.Services;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AmazonWeb.Core.UnitTests
{
    public class CartServiceUnitTests
    {
        private readonly Mock<ICartRepository> _cartRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ICartService _cartService;
        private readonly Fixture _fixture;

        public CartServiceUnitTests()
        {
            _cartRepositoryMock = new Mock<ICartRepository>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _productServiceMock = new Mock<IProductService>();
            _configurationMock = new Mock<IConfiguration>();
            _fixture = new Fixture();

            _configurationMock.Setup(c => c["JwtSettings:Issuer"]).Returns("https://api.amazonclone.com");

            _cartService = new CartService(
                _cartRepositoryMock.Object,
                _userManagerMock.Object,
                _productServiceMock.Object,
                _configurationMock.Object);
        }

        #region GetCartByUserIdAsync
        [Fact]
        public async Task GetCartByUserIdAsync_EmptyUserID_ReturnsNull()
        {
            var result = await _cartService.GetCartByUserIdAsync(Guid.Empty);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCartByUserIdAsync_UnknownUserID_ReturnsEmptyCartResponse()
        {
            Guid newId = Guid.NewGuid();
            _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(newId))
                               .ReturnsAsync((IEnumerable<CartItem>?)null);

            var result = await _cartService.GetCartByUserIdAsync(newId);

            result.Should().BeEquivalentTo(new CartResponse());
        }
        #endregion

        #region AddOrUpdateItemAsync
        [Fact]
        public async Task AddOrUpdateItemAsync_UserNotFound_ReturnsNull()
        {
            var request = new CartRequest { ProductId = Guid.NewGuid(), Quantity = 1 };
            Guid userId = Guid.NewGuid();

            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync((ApplicationUser?)null);

            var result = await _cartService.AddOrUpdateItemAsync(userId, request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddOrUpdateItemAsync_ValidRequest_ReturnsCartResponse()
        {
            var request = _fixture.Build<CartRequest>().Create();
            Guid userId = Guid.NewGuid();

            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { Id = userId });

            var updatedItems = new List<CartItem>
            {
                new CartItem { ProductId = request.ProductId, Quantity = 2, Product = new Product { Name = "Test", Price = 100, ImageUrl = "img.png" } }
            };

            _cartRepositoryMock.Setup(r => r.UpdateQuantityAsync(userId, request.ProductId, request.Quantity))
                               .ReturnsAsync(updatedItems);

            var result = await _cartService.AddOrUpdateItemAsync(userId, request);

            result.Should().NotBeNull();
            result!.Items.Should().ContainSingle(i => i.ProductId == request.ProductId && i.Quantity == 2);
        }
        #endregion

        #region RemoveItemAsync
        [Fact]
        public async Task RemoveItemAsync_UserNotFound_ReturnsFalse()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync((ApplicationUser?)null);

            var result = await _cartService.RemoveItemAsync(userId, productId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveItemAsync_ValidUser_ReturnsTrue()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync(new ApplicationUser { Id = userId });

            _cartRepositoryMock.Setup(r => r.RemoveItemAsync(userId, productId)).ReturnsAsync(true);

            var result = await _cartService.RemoveItemAsync(userId, productId);

            result.Should().BeTrue();
        }
        #endregion

        #region ClearCartAsync
        [Fact]
        public async Task ClearCartAsync_UserNotFound_ReturnsFalse()
        {
            var userId = Guid.NewGuid();
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync((ApplicationUser?)null);

            var result = await _cartService.ClearCartAsync(userId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ClearCartAsync_ValidUser_ReturnsTrue()
        {
            var userId = Guid.NewGuid();
            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync(new ApplicationUser { Id = userId });

            _cartRepositoryMock.Setup(r => r.ClearCartAsync(userId)).ReturnsAsync(true);

            var result = await _cartService.ClearCartAsync(userId);

            result.Should().BeTrue();
        }
        #endregion

        #region MergeCartAsync
        [Fact]
        public async Task MergeCartAsync_GuestItemsNull_ReturnsCurrentCart()
        {
            var userId = Guid.NewGuid();
            _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId))
                               .ReturnsAsync(new List<CartItem>());

            var result = await _cartService.MergeCartAsync(userId, null);

            result.Should().NotBeNull();
            result!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task MergeCartAsync_GuestItemsEmpty_ReturnsCurrentCart()
        {
            var userId = Guid.NewGuid();
            _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId))
                               .ReturnsAsync(new List<CartItem>());

            var result = await _cartService.MergeCartAsync(userId, new List<CartRequest>());

            result.Should().NotBeNull();
            result!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task MergeCartAsync_ValidGuestItems_AddsToCart()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var guestItems = new List<CartRequest>
            {
                new CartRequest { ProductId = productId, Quantity = 2 }
            };

            _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId))
                               .ReturnsAsync(new List<CartItem>());

            _userManagerMock.Setup(m => m.FindByIdAsync(userId.ToString()))
                            .ReturnsAsync(new ApplicationUser { Id = userId });

            _cartRepositoryMock.Setup(r => r.UpdateQuantityAsync(userId, productId, 2))
                               .ReturnsAsync(new List<CartItem>
                               {
                                   new CartItem { ProductId = productId, Quantity = 2, Product = new Product { Name = "Test", Price = 100, ImageUrl = "img.png" } }
                               });

            var result = await _cartService.MergeCartAsync(userId, guestItems);

            result.Should().NotBeNull();
            result!.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);
        }
        #endregion
    }
}
