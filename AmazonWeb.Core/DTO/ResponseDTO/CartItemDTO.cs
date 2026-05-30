namespace AmazonWeb.Core.DTO.ResponseDTO
{
    public class CartItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        // 🎯 These pull dynamically from the joined Product table at runtime!
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string imageUrl { get; set; } = string.Empty;

        public int TotalPrice => Quantity * Price;
    }
}