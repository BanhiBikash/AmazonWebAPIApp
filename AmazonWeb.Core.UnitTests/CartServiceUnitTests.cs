using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Identities;
using AmazonWeb.Core.Domain.RepositoryContract;
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

            // Setup IConfiguration mock to return a dummy base URL for the central URL sanitizer
            _configurationMock
                .Setup(c => c["JwtSettings:Issuer"])
                .Returns("https://api.amazonclone.com");

            //CREATING THE CART SERVICE WITH MOCKS
            _cartService = new CartService(
                _cartRepositoryMock.Object,
                _userManagerMock.Object,
                _productServiceMock.Object,
                _configurationMock.Object);
        }

        //tests
        #region GetCartByUserIdAsyncs
        [Fact]
        public async Task GetCartByUserIdAsync_EmptyUserID_ReturnsNull()
        {
            //Arrange
            Guid emptyUserId = Guid.Empty;

            //Act
            var result = await _cartService.GetCartByUserIdAsync(emptyUserId);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetCartByUserIdAsync_UnknownUserID_ReturnsNull()
        {
            //Arrange
            Guid newId = Guid.NewGuid();
            _cartRepositoryMock.Setup(repo => repo.GetCartByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((IEnumerable<CartItem>?)null);

            //Act
            var result = await _cartService.GetCartByUserIdAsync(newId);

            //Assert
            result.Should().BeEquivalentTo(new CartResponse());
        }

        #endregion
    }
}
