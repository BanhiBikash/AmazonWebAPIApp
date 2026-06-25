using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.Models;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        #region AddProduct
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

        #endregion
    }
}