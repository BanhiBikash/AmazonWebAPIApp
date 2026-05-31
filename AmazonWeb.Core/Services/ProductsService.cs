using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.ServiceContracts;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using Microsoft.Extensions.Configuration; // 🎯 ADDED: For accessing configuration strings safely

namespace AmazonWeb.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration; // 🎯 ADDED: To pull your Centralized Identity Server Issuer URL

        public ProductService(IProductRepository productRepository, IFileService fileService, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _fileService = fileService;
            _configuration = configuration;
        }

        // Check DB health
        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _productRepository.IsDatabaseAliveAsync();
        }

        // Get product by Id
        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            // Validate input
            if (id == Guid.Empty)
                throw new ArgumentException("Product Id cannot be empty.");

            Product? product = await _productRepository.GetByIdAsync(id);

            product = product != null && !product.IsDeleted ? product : null;

            // 🎯 FIXED: Map and append full base URL dynamically here
            return product != null ? FormatProductResponseWithUrl(Product.ToProductResponse(product)) : null;
        }

        // Get all products
        public async Task<IEnumerable<ProductResponse>?> GetAllProductsAsync()
        {
            IEnumerable<Product>? products = await _productRepository.GetAllAsync();

            if (products == null) return null;

            // 🎯 FIXED: Converts entities, screens out soft-deleted products, and updates image URL roots cleanly
            return products
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse)
                .Select(FormatProductResponseWithUrl);
        }

        // Add product-work continue
        public async Task<Product> AddProductAsync(ProductAddRequest productAddRequest)
        {
            if (productAddRequest == null)
                throw new ArgumentNullException(nameof(productAddRequest));

            //create product id
            Guid productId = Guid.NewGuid();

            //Takes the file and name and saves it also Build relative URL for client access
            var relativeUrl = await _fileService.UploadThumbnailAsync(productAddRequest.Thumbnail, productId);

            // Map to Product entity
            var product = new Product
            {
                Id = productId,
                Name = productAddRequest.Name,
                Price = productAddRequest.Price,
                InStock = productAddRequest.InStock,
                Stock = productAddRequest.Stock,
                Description = productAddRequest.Description,
                ImageUrl = relativeUrl,
                Category = productAddRequest.Category != null ? productAddRequest.Category : ProductCategory.Common,
                SubCategory = productAddRequest.SubCategory != null ? productAddRequest.SubCategory : ProductSubCategory.Common,
                IsDeleted = false
            }; ;

            return await _productRepository.AddAsync(product);
        }

        // Update product
        public async Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
        {
            if (productUpdateRequest == null)
                throw new ArgumentNullException(nameof(productUpdateRequest));

            ProductResponse? product = await GetProductByIdAsync(productUpdateRequest.Id);

            // product does not exist or is deleted
            if (product == null)
            {
                return null;
            }

            product = ProductUpdateRequest.ApplyUpdate(product, productUpdateRequest);

            // Handle thumbnail logic ---
            string finalImageUrl = product.ImageUrl; // Keep original by default

            if (productUpdateRequest.Thumbnail != null && productUpdateRequest.Thumbnail.Length > 0)
            {
                // Upload new file and retrieve relative URL path
                finalImageUrl = await _fileService.UploadThumbnailAsync(productUpdateRequest.Thumbnail, product.Id);
            }

            Product? productToUpdate = new Product()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                InStock = product.InStock,
                Stock = product.Stock,
                Description = product.Description,
                ImageUrl = finalImageUrl,
                Category = Enum.Parse<ProductCategory>(product.Category),
                SubCategory = string.IsNullOrEmpty(product.SubCategory) ? ProductSubCategory.Toy_Puzzles : Enum.Parse<ProductSubCategory>(product.SubCategory),
                IsDeleted = product.IsDeleted
            };

            Product? UpdatedProduct = await _productRepository.UpdateAsync(productToUpdate);

            // 🎯 FIXED: Cleans updated paths instantly before bubbling up to controller endpoints
            return UpdatedProduct != null ? FormatProductResponseWithUrl(Product.ToProductResponse(UpdatedProduct)) : null;
        }

        // Delete product
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Product Id cannot be empty.");

            ProductResponse? product = await GetProductByIdAsync(id);
            if (product == null)
                return false;

            // Soft delete by setting IsDeleted to true
            product.IsDeleted = true;

            // Safely parse Category and SubCategory from string → enum
            var productToDelete = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                InStock = product.InStock,
                Stock = product.Stock,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                IsDeleted = product.IsDeleted,

                Category = Enum.TryParse<ProductCategory>(product.Category, true, out var parsedCategory)
                                ? parsedCategory
                                : ProductCategory.Common, // fallback

                SubCategory = string.IsNullOrWhiteSpace(product.SubCategory)
                                ? ProductSubCategory.Common   // fallback
                                : Enum.TryParse<ProductSubCategory>(product.SubCategory, true, out var parsedSubCategory)
                                    ? parsedSubCategory
                                    : ProductSubCategory.Common // fallback
            };

            await _productRepository.UpdateAsync(productToDelete);
            return true;
        }

        // Get products by price range
        public async Task<IEnumerable<ProductResponse>?> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0)
                throw new ArgumentException("Price range cannot be negative.");
            if (minPrice > maxPrice)
                throw new ArgumentException("Min price cannot be greater than max price.");

            var products = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse)
                .Select(FormatProductResponseWithUrl); // 🎯 FIXED
        }

        // Get products by category
        public async Task<IEnumerable<ProductResponse>?> GetProductsByCategoryAsync(ProductCategory category)
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse)
                .Select(FormatProductResponseWithUrl); // 🎯 FIXED
        }

        // Get products by subcategory
        public async Task<IEnumerable<ProductResponse>?> GetProductsBySubCategoryAsync(ProductSubCategory subCategory)
        {
            var products = await _productRepository.GetBySubCategoryAsync(subCategory);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse)
                .Select(FormatProductResponseWithUrl); // 🎯 FIXED
        }

        // Search products by name
        public async Task<IEnumerable<ProductResponse>?> SearchProductsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search term cannot be empty.");

            var products = await _productRepository.SearchByNameAsync(name);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse)
                .Select(FormatProductResponseWithUrl); // 🎯 FIXED
        }

        /* ==========================================================================
           🎯 THE CENTRALIZED URL SANITIZER (Single point of truth)
           ========================================================================== */
        private ProductResponse FormatProductResponseWithUrl(ProductResponse response)
        {
            if (response == null) return response;

            string? baseUrl = _configuration.GetValue<string>("JwtSettings:Issuer");

            if (!string.IsNullOrEmpty(baseUrl) &&
                !string.IsNullOrEmpty(response.ImageUrl) &&
                !response.ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                response.ImageUrl = baseUrl.TrimEnd('/') + "/" + response.ImageUrl.TrimStart('/');
            }

            return response;
        }
    }
}