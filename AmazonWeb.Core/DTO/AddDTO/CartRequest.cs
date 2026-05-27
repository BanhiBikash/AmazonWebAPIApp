using System;
using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.DTO.AddDTO
{
    public class CartRequest
    {
        [Required(ErrorMessage = "Product identification context is required.")]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity allocation parameters must be restricted between 1 and 100 units.")]
        public int Quantity { get; set; }
    }
}