using System.ComponentModel.DataAnnotations;

namespace AmazonWeb.Core.Models
{
    public class ItemData
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product cost URL is required.")]
        public double unitPrice { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }
    }
}
