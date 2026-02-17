namespace MS2.WebApp.Models
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public int TotalItems => Items.Sum(i => i.Quantity);

        public decimal TotalAmount => Items.Sum(i => i.Subtotal);

        public bool IsEmpty => !Items.Any();
    }
}
