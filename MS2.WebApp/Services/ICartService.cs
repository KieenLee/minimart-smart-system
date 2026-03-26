using MS2.WebApp.Models;

namespace MS2.WebApp.Services
{
    public interface ICartService
    {
        /// <summary>Get cart items from session.</summary>
        List<CartItemViewModel> GetCart();

        /// <summary>Add a product or increment its quantity. Returns error message or null on success.</summary>
        Task<string?> AddToCartAsync(int productId, int quantity = 1);

        /// <summary>Update item quantity directly. Pass 0 to remove. Returns error message or null on success.</summary>
        Task<string?> UpdateQuantityAsync(int productId, int quantity);

        /// <summary>Remove a product from the cart.</summary>
        void RemoveItem(int productId);

        /// <summary>Clear all items from cart.</summary>
        void ClearCart();

        /// <summary>Get total item count.</summary>
        int GetCartCount();

        /// <summary>Get total price.</summary>
        decimal GetCartTotal();
    }
}
