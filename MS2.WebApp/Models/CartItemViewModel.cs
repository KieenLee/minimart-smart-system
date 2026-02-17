namespace MS2.WebApp.Models
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }

        public decimal Subtotal => Price * Quantity;
    }
}
