using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.ServiceContracts;

namespace AmazonWeb.Core.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IFileService _fileService;

        public ProductService(IProductRepository productRepository, IFileService fileService) 
        {
            _productRepository = productRepository;
            _fileService = fileService;
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

            // Check if database is alive before attempting to retrieve product
            if (!(await IsDatabaseAliveAsync()))
            {
                return null;
            }

            Product? product =  await _productRepository.GetByIdAsync(id);

            product = product != null && !product.IsDeleted ? product : null;

            return product != null ? Product.ToProductResponse(product) : null;
        }

        // Get all products
        public async Task<IEnumerable<ProductResponse>?> GetAllProductsAsync()
        {
            // Check if database is alive before attempting to retrieve product
            if (!(await IsDatabaseAliveAsync()))
            {
                return null;
            }

            IEnumerable<Product>? products = await _productRepository.GetAllAsync();

            return products != null ? products.Select(Product.ToProductResponse).Where(products=>products.IsDeleted==false) : null;
        }

        // Add product-work continue
        public async Task<Product> AddProductAsync(ProductAddRequest productAddRequest)
        {
            if (productAddRequest == null)
                throw new ArgumentNullException(nameof(productAddRequest));

            //create product id
            Guid productId = Guid.NewGuid();

            //Takes the file and name and saves it also Build relative URL for client access
            var relativeUrl = await _fileService.UploadThumbnailAsync(productAddRequest.Thumbnail,productId);

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
                Category = productAddRequest.Category !=null?productAddRequest.Category:ProductCategory.Common,
                SubCategory = productAddRequest.SubCategory!=null?productAddRequest.SubCategory:ProductSubCategory.Common,
                IsDeleted = false
            }; ;

            return await _productRepository.AddAsync(product);
        }

        // Update product
        public async Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
        {
            if (productUpdateRequest == null)
                throw new ArgumentNullException(nameof(productUpdateRequest));

            // Check if database is alive before attempting to retrieve product
            if (!(await IsDatabaseAliveAsync()))
            {
                return null;
            }

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

            return UpdatedProduct != null ? Product.ToProductResponse(UpdatedProduct) : null;
        }

        // Delete product
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Product Id cannot be empty.");

            // Check if database is alive before attempting to retrieve product
            if (!(await IsDatabaseAliveAsync()))
                return false;

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
                .Select(Product.ToProductResponse);
        }

        // Get products by category
        public async Task<IEnumerable<ProductResponse>?> GetProductsByCategoryAsync(ProductCategory category)
        {
            var products = await _productRepository.GetByCategoryAsync(category);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse);
        }

        // Get products by subcategory
        public async Task<IEnumerable<ProductResponse>?> GetProductsBySubCategoryAsync(ProductSubCategory subCategory)
        {
            var products = await _productRepository.GetBySubCategoryAsync(subCategory);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse);
        }

        // Search products by name
        public async Task<IEnumerable<ProductResponse>?> SearchProductsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Search term cannot be empty.");

            var products = await _productRepository.SearchByNameAsync(name);
            return products?
                .Where(p => !p.IsDeleted)
                .Select(Product.ToProductResponse);
        }
    }
}
