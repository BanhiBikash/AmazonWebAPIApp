using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.Domain.Entities
{
    public class Product
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public bool InStock { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? ImageUrl { get; set; }

        [Required]
        public ProductCategory? Category { get; set; }

        //[Required]
        public ProductSubCategory? SubCategory { get; set; }
    }
}
