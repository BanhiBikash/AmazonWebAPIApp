using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.DeleteDTO
{
    public class ProductDeleteRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Delete flag is required")]
        public bool IsDeleted { get; set; }
    }
}
