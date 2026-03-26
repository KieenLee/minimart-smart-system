using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MS2.WebApp.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Index");

        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Bạn đã đăng xuất thành công.";
            return RedirectToPage("/Index");
        }
    }
}
