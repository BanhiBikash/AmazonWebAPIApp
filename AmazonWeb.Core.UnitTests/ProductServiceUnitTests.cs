using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.Services;
using Castle.Core.Configuration;
using Moq;

namespace AmazonWeb.Core.UnitTests
{
    public class ProductServiceUnitTests
    {
        private readonly IProductService _productService; 
        private readonly IProductRepository _productRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;

        public ProductServiceUnitTests(IFileService fileService, IConfiguration configuration)
        {
            //injecting necessary dependencies into the constructor
            _fileService = fileService;
            _configuration = configuration;

            _mockProductRepository = new Mock<IProductRepository>();    //created mock repository of Product type
            _productRepository = _mockProductRepository.Object; //assigned mock obj to product repository
            _productService = new ProductService(_productRepository, _fileService, _configuration); //created product service by passing mocked product repository
        }
    }
}
