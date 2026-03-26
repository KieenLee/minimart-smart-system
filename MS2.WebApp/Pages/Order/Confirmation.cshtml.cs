using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MS2.WebApp.Pages.Order
{
    public class ConfirmationModel : PageModel
    {
        public int OrderId { get; set; }

        public IActionResult OnGet(int orderId)
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToPage("/Index");
            OrderId = orderId;
            return Page();
        }
    }
}
