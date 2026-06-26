using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.Models;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.Services;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IO;
using Xunit;

namespace AmazonWeb.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly ProductService _productService;
        private readonly Fixture _fixture;

        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _fileServiceMock = new Mock<IFileService>();
            _configurationMock = new Mock<IConfiguration>();
             _fixture = new Fixture();

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

        #region AddProduct
        //return null exception on null request
        [Fact]
        public async Task AddProduct_RequestisNull_ReturnsNull()
        {
            //Act
            ProductAddRequest? productAddRequest = null;

            //Arrange
            Func<Task> action = async () => await _productService.AddProductAsync(productAddRequest);

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>(nameof(productAddRequest));
        }

        [Fact]
        public async Task AddProduct_ValidRequest_ReturnsProductResponseDTO()
        {
            // Arrange
            ProductAddRequest productAddRequest = _fixture.Build<ProductAddRequest>().With(field=>field.Thumbnail ,CreateFakeFormFile()).Create();
            Product product = ProductAddRequest.ToProduct(productAddRequest);

            //Act
            _productRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Product>())).ReturnsAsync(product);
            Product? productResponse = await _productService.AddProductAsync(productAddRequest);

            //Assert
            productResponse.Id.Should().NotBeEmpty();
            productResponse.Should().Be(product);
        }
        #endregion

        #region GetProductById
        [Fact]
        public async Task GetProductById_IdIsEmpty_ThrowsArgumentException()
        {
            Func<Task> action = async () => await _productService.GetProductByIdAsync(Guid.Empty);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetProductById_ProductIsDeleted_ReturnsNull()
        {
            var product = _fixture.Build<Product>()
                .With(p => p.IsDeleted, true)
                .Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            var result = await _productService.GetProductByIdAsync(product.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProductById_ValidProduct_ReturnsProductResponse()
        {
            // Arrange
            Product product = new Product()
            {
                Id = Guid.NewGuid(),
                Name = "Gaming Laptop",
                Price = 1200,
                Discount = 10, // 10% off
                InStock = true,
                Stock = 25,
                Description = "High-performance laptop with RTX graphics",
                ImageUrl = "http://cdn.amazonclone.com/images/laptop.png",
                Category = ProductCategory.Mobiles,
                SubCategory = ProductSubCategory.Mobile_Smartphones,
                IsDeleted = false
            }; 

            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
            _configurationMock.Setup(c => c["JwtSettings:Issuer"])
                  .Returns("https://cdn.amazonclone.com");

            // Build expected response using the same sanitizer logic
            var expected = Product.ToProductResponse(product);

            // Act
            var result = await _productService.GetProductByIdAsync(product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(_productService.FormatProductResponseWithUrl(expected));
        }

        #endregion

        #region UpdateProduct
        [Fact]
        public async Task UpdateProduct_RequestIsNull_ThrowsArgumentNullException()
        {
            Func<Task> action = async () => await _productService.UpdateProductAsync(null);

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_ReturnsNull()
        {
            var request = _fixture.Build<ProductUpdateRequest>().With(f=>f.Thumbnail, CreateFakeFormFile()).Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            var result = await _productService.UpdateProductAsync(request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProduct_ValidRequest_ReturnsUpdatedResponse()
        {
            var product = _fixture.Build<Product>()
                .With(p => p.ImageUrl, "img.png")
                .Create();

            var request = _fixture.Build<ProductUpdateRequest>().With(f=>f.Id, product.Id).With(f=>f.Thumbnail, CreateFakeFormFile()).With(f=>f.ImageUrl, "img.png").Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(product);

            var result = await _productService.UpdateProductAsync(request);
            ProductResponse expected = Product.ToProductResponse(product);
            expected.Should().NotBeNull();
            expected.Should().BeEquivalentTo(Product.ToProductResponse(product)); // ApplyUpdate logic preserves mapping
        }
        #endregion

        #region DeleteProduct
        [Fact]
        public async Task DeleteProduct_IdIsEmpty_ThrowsArgumentException()
        {
            Func<Task> action = async () => await _productService.DeleteProductAsync(Guid.Empty);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_ReturnsFalse()
        {
            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            var result = await _productService.DeleteProductAsync(Guid.NewGuid());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteProduct_ValidProduct_ReturnsTrue()
        {
            //arrange
            var product = _fixture.Build<Product>()
                .With(p => p.IsDeleted, false)
                .Create();

            var deletedProduct = _fixture.Build<Product>()
                .With(p => p.IsDeleted, true)
                .Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(deletedProduct);

            //act
            var result = await _productService.DeleteProductAsync(product.Id);

            //assert
            result.Should().BeTrue();
        }
        #endregion

        #region DeductStock
        [Fact]
        public async Task DeductStock_IdIsEmpty_ThrowsArgumentException()
        {
            Func<Task> action = async () => await _productService.DeductProductStockAsync(Guid.Empty, 1);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeductStock_ProductNotFound_ThrowsInvalidOperation()
        {
            _productRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            Func<Task> action = async () => await _productService.DeductProductStockAsync(Guid.NewGuid(), 1);

            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeductStock_InsufficientStock_ThrowsInvalidOperation()
        {
            var product = _fixture.Build<Product>()
                .With(p => p.Stock, 1)
                .Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

            Func<Task> action = async () => await _productService.DeductProductStockAsync(product.Id, 5);

            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeductStock_ValidRequest_UpdatesStock()
        {
            var product = _fixture.Build<Product>()
                .With(p => p.Stock, 10)
                .Create();

            _productRepositoryMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
            _productRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(product);

            await _productService.DeductProductStockAsync(product.Id, 5);

            product.Stock.Should().Be(5);
        }
        #endregion


        private IFormFile CreateFakeFormFile(string fileName = "test.png", string content = "fake image content")
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            return new FormFile(stream, 0, stream.Length, "Thumbnail", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
        }

    }
}