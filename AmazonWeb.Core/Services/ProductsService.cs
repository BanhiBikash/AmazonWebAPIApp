using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.Domain.RepositoryContract;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.Services
{
    public class ProductsService
    {
        private readonly IProductRepository _productRepository;

        public ProductsService(ProductsService productsService) 
        {
            _productRepository = productsService._productRepository;
        }
        
        // Check DB health
        public async Task<bool> IsDatabaseAliveAsync()
        {
            return await _productRepository.IsDatabaseAliveAsync();
        }

        // Get product by Id
        public async Task<ProductResponse?> GetProductByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Product Id cannot be empty.");

            Product? product =  await _productRepository.GetByIdAsync(id);

            return product != null ? Product.ToProductResponse(product) : null;
        }

        // Get all products
        public async Task<IEnumerable<ProductResponse>?> GetAllProductsAsync()
        {
            IEnumerable<Product>? products = await _productRepository.GetAllAsync();

            return products != null ? products.Select(Product.ToProductResponse) : null;
        }

        // Add product
        public async Task<Product> AddProductAsync(ProductAddRequest productAddRequest)
        {
            if (productAddRequest == null)
                throw new ArgumentNullException(nameof(productAddRequest));

            Product product = ProductAddRequest.ToProduct(productAddRequest);

            Product? productAdded = await _productRepository.AddAsync(product);

            return productAdded;
        }

        // Update product
        public async Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
        {
            if (productUpdateRequest == null)
                throw new ArgumentNullException(nameof(productUpdateRequest));

            ProductResponse? product = await GetProductByIdAsync(productUpdateRequest.Id);

            product = ProductUpdateRequest.ApplyUpdate(product, productUpdateRequest);

            Product? productToUpdate = new Product()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                InStock = product.InStock,
                Stock = product.Stock,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                Category = Enum.Parse<ProductCategory>(product.Category),
                SubCategory = string.IsNullOrEmpty(product.SubCategory) ? ProductSubCategory.Toy_Puzzles : Enum.Parse<ProductSubCategory>(product.SubCategory),
                IsDeleted = product.IsDeleted
            } ;

            Product ? UpdatedProduct = await _productRepository.UpdateAsync(productToUpdate);

            return UpdatedProduct != null ? Product.ToProductResponse(UpdatedProduct) : null;
        }

        // Delete product
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Product Id cannot be empty.");

            return await _productRepository.DeleteAsync(id);
        }
    }
}
