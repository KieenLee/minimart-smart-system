using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        public ChangePasswordModel(IAuthService authService) { _authService = authService; }

        [BindProperty]
        public ChangePasswordViewModel Input { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (!HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToPage("/Account/Login");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid) return Page();

            var error = await _authService.ChangePasswordAsync(userId.Value, Input.CurrentPassword, Input.NewPassword);
            if (error != null) { ErrorMessage = error; return Page(); }

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToPage("/Account/Profile");
        }
    }
}
