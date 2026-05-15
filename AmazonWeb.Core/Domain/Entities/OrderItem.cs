using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AmazonWeb.Core.Domain.Entities
{
    public class OrderItem
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 100,ErrorMessage ="Orders are alloweed in the range of 1-100")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int UnitPrice { get; set; }
    }
}
