using Microsoft.AspNetCore.Mvc;
using MS2.DataAccess.Interfaces;
using MS2.WebApp.Models;
using System.Text.Json;

namespace MS2.WebApp.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "Cart";

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            // Kiểm tra sản phẩm có tồn tại không
            var product = await _unitOfWork.Products.GetByIdAsync(productId);

            if (product == null || !product.IsActive)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                return RedirectToAction("Index", "Products");
            }

            // Lấy giỏ hàng hiện tại
            var cart = GetCart();

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                // Cập nhật số lượng
                var newQuantity = existingItem.Quantity + quantity;

                if (newQuantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Số lượng vượt quá tồn kho. Còn lại: {product.Stock}";
                    return RedirectToAction("Index", "Products");
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                // Kiểm tra tồn kho
                if (quantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Số lượng vượt quá tồn kho. Còn lại: {product.Stock}";
                    return RedirectToAction("Index", "Products");
                }

                // Thêm mới vào giỏ
                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl
                });
            }

            // Lưu giỏ hàng vào session
            SaveCart(cart);

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Products");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
            {
                return RedirectToAction("RemoveItem", new { productId });
            }

            // Kiểm tra tồn kho
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product != null && quantity > product.Stock)
            {
                TempData["ErrorMessage"] = $"Số lượng vượt quá tồn kho. Còn lại: {product.Stock}";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                if (product != null)
                {
                    item.Stock = product.Stock;
                }
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã cập nhật số lượng!";
            }

            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        // POST: Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction("Index");
        }

        #region Helper Methods

        private CartViewModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            return JsonSerializer.Deserialize<CartViewModel>(cartJson) ?? new CartViewModel();
        }

        private void SaveCart(CartViewModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        #endregion
    }
}
