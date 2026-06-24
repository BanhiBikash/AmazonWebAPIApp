using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.Models;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AmazonWeb.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _configurationMock = new Mock<IConfiguration>();

            // Setup IConfiguration mock to return a dummy base URL for the central URL sanitizer
            _configurationMock
                .Setup(c => c["JwtSettings:Issuer"])
                .Returns("https://api.amazonclone.com");

            _productService = new ProductService(
                _productRepositoryMock.Object,
                _fileServiceMock.Object,
                _configurationMock.Object
            );
        }

        #region GetProductByIdAsync Tests

        [Fact]
        public async Task GetProductByIdAsync_WhenIdIsEmpty_ShouldThrowArgumentException()
        {
            // Arrange
            Guid emptyId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _productService.GetProductByIdAsync(emptyId);
            });
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductIsDeleted_ShouldReturnNull()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var deletedProduct = new Product { Id = productId, Name = "Test", IsDeleted = true };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(deletedProduct);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductIsValid_ShouldReturnFormattedUrl()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Echo Dot",
                ImageUrl = "uploads/images/echo.jpg",
                IsDeleted = false
            };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Echo Dot", result.Name);
            // Validates that FormatProductResponseWithUrl worked perfectly
        }

        #endregion

        #region DeductProductStockAsync Tests

        [Fact]
        public async Task DeductProductStockAsync_WhenStockIsInsufficient_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Laptop", Stock = 5, IsDeleted = false };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await _productService.DeductProductStockAsync(productId, 10); // Deducting more than 5
            });

            Assert.Contains("Insufficient warehouse stock", exception.Message);
        }

        [Fact]
        public async Task DeductProductStockAsync_WhenStockIsSufficient_ShouldReduceStockAndCallUpdate()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Laptop", Stock = 10, IsDeleted = false };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(product);

            _productRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Product>()))
                .ReturnsAsync(product);

            // Act
            await _productService.DeductProductStockAsync(productId, 3);

            // Assert
            Assert.Equal(7, product.Stock); // 10 - 3
            _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => p.Stock == 7)), Times.Once);
        }

        #endregion

        #region CheckItemDataSanctity Tests

        [Fact]
        public async Task CheckItemDataSanctity_WhenPriceMismatches_ShouldReturnFalseAndNotDeductStock()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var liveProduct = new Product { Id = productId, Name = "Book", Price = 499, Stock = 20, IsDeleted = false };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(liveProduct);

            var clientItems = new List<ItemData>
            {
                new ItemData { ProductId = productId, unitPrice = 299.00d, Quantity = 2 } // Spoofed/incorrect price
            };

            // Act
            bool result = await _productService.CheckItemDataSanctity(clientItems);

            // Assert
            Assert.False(result);
            // Verify stock deduction was never called because data truth failed
            _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CheckItemDataSanctity_WhenAllDataIsValid_ShouldDeductStockAndReturnTrue()
        {
            // Arrange
            Guid productId = Guid.NewGuid();
            var liveProduct = new Product { Id = productId, Name = "Book", Price = 50, Stock = 10, IsDeleted = false };

            _productRepositoryMock
                .Setup(repo => repo.GetByIdAsync(productId))
                .ReturnsAsync(liveProduct);

            var clientItems = new List<ItemData>
            {
                new ItemData { ProductId = productId, unitPrice = 50.00d, Quantity = 2 }
            };

            // Act
            bool result = await _productService.CheckItemDataSanctity(clientItems);

            // Assert
            Assert.True(result);
            Assert.Equal(8, liveProduct.Stock); // Verified stock reduced from 10 to 8
            _productRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => p.Stock == 8)), Times.Once);
        }

        #endregion
    }
}