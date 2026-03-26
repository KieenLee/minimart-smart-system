using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Order
{
    public class CheckoutModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CheckoutModel(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        [BindProperty]
        public CheckoutViewModel Input { get; set; } = new();

        public List<CartItemViewModel> CartItems { get; set; } = new();
        public decimal CartTotal { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            CartItems = _cartService.GetCart();
            CartTotal = _cartService.GetCartTotal();

            if (!CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToPage("/Cart/Index");
            }

            // Pre-fill with profile info from session
            Input.ReceiverName = HttpContext.Session.GetString("FullName") ?? "";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            CartItems = _cartService.GetCart();
            CartTotal = _cartService.GetCartTotal();

            if (!CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToPage("/Cart/Index");
            }

            if (!ModelState.IsValid) return Page();

            var (orderId, error) = await _orderService.PlaceOrderAsync(
                userId.Value,
                Input.ReceiverName, Input.PhoneNumber,
                Input.DeliveryAddress, Input.Note, CartItems);

            if (error != null) { ErrorMessage = error; return Page(); }

            _cartService.ClearCart();
            TempData["Success"] = "Đặt hàng thành công! Chúng tôi sẽ liên hệ sớm.";
            return RedirectToPage("/Order/Confirmation", new { orderId });
        }
    }
}
