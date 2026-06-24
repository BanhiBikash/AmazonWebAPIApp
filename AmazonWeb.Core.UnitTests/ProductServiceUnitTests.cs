using AmazonWeb.Core.ServiceContracts.ProductContracts;

namespace AmazonWeb.Core.UnitTests
{
    public class ProductServiceUnitTests
    {
        private readonly IProductService _productService; 
        
        public ProductServiceUnitTests(IProductService productService)
        {
            // Initialize the product service with a mock or real implementation
            _productService = productService;
        }
    }
}
