using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.Models.Entities;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Order
{
    public class HistoryModel : PageModel
    {
        private readonly IOrderService _orderService;
        public HistoryModel(IOrderService orderService) { _orderService = orderService; }

        public List<MS2.Models.Entities.Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            Orders = await _orderService.GetOrdersByCustomerAsync(userId.Value);
            return Page();
        }
    }
}
