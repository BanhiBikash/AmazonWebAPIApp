using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmazonWeb.Core.ServiceContracts.ProductContracts
{
    /// <summary>
    /// Defines the business logic contract for managing system inventory products.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Evaluates current data stream health connectivity vectors.
        /// </summary>
        Task<bool> IsDatabaseAliveAsync();

        /// <summary>
        /// Pulls a single product record filtered by its unique identifier.
        /// </summary>
        Task<ProductResponse?> GetProductByIdAsync(Guid id);

        /// <summary>
        /// Pulls all active, non-soft-deleted products from the persistence cluster.
        /// </summary>
        Task<IEnumerable<ProductResponse>?> GetAllProductsAsync();

        /// <summary>
        /// Commits a fresh product entity alongside binary image assets.
        /// </summary>
        Task<Product> AddProductAsync(ProductAddRequest productAddRequest);

        /// <summary>
        /// Updates an existing product entity state mutation matrix.
        /// </summary>
        Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productUpdateRequest);

        /// <summary>
        /// Flag-triggers a logical soft delete status mutation on a target product tracking row.
        /// </summary>
        Task<bool> DeleteProductAsync(Guid id);

        /// <summary>
        /// Pulls active products matching a designated monetary parameter range.
        /// </summary>
        Task<IEnumerable<ProductResponse>?> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);

        /// <summary>
        /// Pulls active products mapping to a matching master domain classification group.
        /// </summary>
        Task<IEnumerable<ProductResponse>?> GetProductsByCategoryAsync(ProductCategory category);

        /// <summary>
        /// Pulls active products mapping to a matching detailed subdomain item tier.
        /// </summary>
        Task<IEnumerable<ProductResponse>?> GetProductsBySubCategoryAsync(ProductSubCategory subCategory);

        /// <summary>
        /// Executes a character string pattern search across all active product title vectors.
        /// </summary>
        Task<IEnumerable<ProductResponse>?> SearchProductsByNameAsync(string name);

        Task DeductProductStockAsync(Guid id, int quantity);

        Task<bool> CheckItemDataSanctity(List<ItemData> itemDatas);
    }
}