using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Order
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderService _orderService;
        public DetailsModel(IOrderService orderService) { _orderService = orderService; }

        public MS2.Models.Entities.Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            Order = await _orderService.GetOrderDetailsAsync(orderId, userId.Value);
            return Page();
        }
    }
}
