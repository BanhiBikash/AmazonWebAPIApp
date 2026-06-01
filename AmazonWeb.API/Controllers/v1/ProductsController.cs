using AmazonWeb.Core.Domain.Entities;
using AmazonWeb.Core.Domain.Enums;
using AmazonWeb.Core.DTO.AddDTO;
using AmazonWeb.Core.DTO.ResponseDTO;
using AmazonWeb.Core.DTO.UpdateDTO;
using AmazonWeb.Core.ServiceContracts.ProductContracts;
using AmazonWeb.Core.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonWeb.API.Controllers.v1
{
    [ApiVersion("1.0")]
    public class ProductsController : CustomControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // 🌐 PUBLIC READ OPERATIONS

        // GET: api/v1/Products
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            if (products == null)
            {
                return StatusCode(503, "Database service is temporarily unavailable.");
            }
            return Ok(products);
        }

        // GET: api/v1/Products/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound($"Product with ID {id} was not found or has been removed.");
            }
            return Ok(product);
        }

        // GET: api/v1/Products/search?name=puzzle
        [HttpGet("search/{name}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> SearchProductsByName([FromRoute] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Search query parameter cannot be empty.");

            var products = await _productService.SearchProductsByNameAsync(name);
            return Ok(products ?? new List<ProductResponse>());
        }

        // GET: api/v1/Products/filter-price?minPrice=10&maxPrice=100
        [HttpGet("filter-price")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsByPrice([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                return BadRequest("Invalid price range arguments provided.");

            var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice);
            return Ok(products ?? new List<ProductResponse>());
        }

        // GET: api/v1/Products/category/Toys
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsByCategory([FromRoute]  string category)
        {
            //parse the route string into your enum handling case insensitivity and invalid values gracefully
            if (!Enum.TryParse<ProductCategory>(category, ignoreCase: true, out var parsedCategory))
            {
                // Returns a clean 400 Bad Request if the category name doesn't exist in your enum
                return BadRequest($"The category value '{category}' is invalid.");
            }

            var products = await _productService.GetProductsByCategoryAsync(parsedCategory);
            return Ok(products ?? new List<ProductResponse>());
        }

        // GET: api/v1/Products/subcategory/Toy_Puzzles
        [HttpGet("subcategory/{subCategory}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProductsBySubCategory([FromRoute] string subCategory)
        {
            //parse the route string into your enum handling case insensitivity and invalid values gracefully
            if (!Enum.TryParse<ProductSubCategory>(subCategory, ignoreCase: true, out var parsedSubCategory))
            {
                // Returns a clean 400 Bad Request if the category name doesn't exist in your enum
                return BadRequest($"The category value '{subCategory}' is invalid.");
            }

            var products = await _productService.GetProductsBySubCategoryAsync(parsedSubCategory);
            return Ok(products ?? new List<ProductResponse>());
        }

        // 🛡️ LOCKED ADMIN OPERATIONS

        // POST: api/v1/Products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductResponse>> AddProduct([FromForm] ProductAddRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Note: Uses [FromForm] so your React client can upload the binary Thumbnail file smoothly
            var createdProduct = await _productService.AddProductAsync(request);

            // Map the domain entity cleanly to a response object for serialization
            var response = AmazonWeb.Core.Domain.Entities.Product.ToProductResponse(createdProduct);

            return CreatedAtAction(nameof(GetProductById), new { id = response.Id }, response);
        }

        // PUT: api/v1/Products
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductResponse>> UpdateProduct([FromForm] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ValidationProblemDetails(ModelState));

            var updatedProduct = await _productService.UpdateProductAsync(request);
            if (updatedProduct == null)
            {
                return NotFound($"Failed to update. Product with ID {request.Id} does not exist or database is offline.");
            }

            return Ok(updatedProduct);
        }

        // DELETE: api/v1/Products/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("A valid product GUID must be supplied.");

            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound($"Product with ID {id} was not found or could not be removed.");
            }

            return NoContent(); // Return standard HTTP 204 for successful deletions
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult GetCategories()
        {
            // 1. Extract all values from the ProductCategory Enum
            var categories = Enum.GetValues(typeof(ProductCategory))
                .Cast<ProductCategory>()
                .Select(c => new
                {
                    Id = (int)c,
                    Name = c.ToString() // e.g., "Electronics"
                })
                .ToList();

            // 2. Extract all values from the ProductSubCategory Enum
            var subCategories = Enum.GetValues(typeof(ProductSubCategory))
                .Cast<ProductSubCategory>()
                .Select(s => new
                {
                    Id = (int)s,
                    Name = s.ToString() // e.g., "Electronics_Mouse" or "Clothing_Shirts"
                })
                .ToList();

            // 3. Return both together so the frontend only needs to make a single API trip
            return Ok(new { categories, subCategories });
        }
    }
}