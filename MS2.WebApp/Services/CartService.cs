using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;
using MS2.WebApp.Models;
using System.Text.Json;

namespace MS2.WebApp.Services
{
    public class CartService : ICartService
    {
        private const string CartSessionKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItemViewModel> GetCart()
        {
            var json = Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItemViewModel>();
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        public async Task<string?> AddToCartAsync(int productId, int quantity = 1)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return "Sản phẩm không tồn tại.";
            if (product.IsActive != true) return "Sản phẩm hiện không còn bán.";
            if (quantity < 1) return "Số lượng không hợp lệ.";

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.ProductId == productId);

            int requestedTotal = (existing?.Quantity ?? 0) + quantity;
            if (requestedTotal > product.Stock)
                return $"Vượt quá số lượng tồn kho. Còn {product.Stock - (existing?.Quantity ?? 0)} sản phẩm có thể thêm.";

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
            return null;
        }

        public async Task<string?> UpdateQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
                return null;
            }

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return "Sản phẩm không tồn tại.";

            if (quantity > product.Stock)
                return $"Số lượng vượt quá tồn kho ({product.Stock} sản phẩm).";

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                item.Stock = product.Stock;
                SaveCart(cart);
            }

            return null;
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        public int GetCartCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.Subtotal);
        }
    }
}
