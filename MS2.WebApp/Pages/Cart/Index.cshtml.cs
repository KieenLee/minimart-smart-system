using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ICartService _cartService;

        public IndexModel(ICartService cartService)
        {
            _cartService = cartService;
        }

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }
        public bool IsLoggedIn { get; set; }

        public void OnGet()
        {
            CartItems = _cartService.GetCart();
            CartTotal = _cartService.GetCartTotal();
            IsLoggedIn = HttpContext.Session.GetInt32("UserId").HasValue;
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int productId, int quantity = 1, string? returnUrl = null)
        {
            var error = await _cartService.AddToCartAsync(productId, quantity);
            if (error != null)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Đã thêm vào giỏ hàng!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, int quantity)
        {
            var error = await _cartService.UpdateQuantityAsync(productId, quantity);
            if (error != null) TempData["Error"] = error;
            return RedirectToPage();
        }

        public IActionResult OnPostRemove(int productId)
        {
            _cartService.RemoveItem(productId);
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToPage();
        }

        public IActionResult OnPostClear()
        {
            _cartService.ClearCart();
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToPage();
        }
    }
}
