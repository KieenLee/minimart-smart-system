using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Account
{
    public class EditProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        public EditProfileModel(IAuthService authService) { _authService = authService; }

        [BindProperty]
        public EditProfileViewModel Input { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null) return RedirectToPage("/Account/Login");

            Input = new EditProfileViewModel
            {
                FullName = user.FullName ?? "",
                Email = user.Email ?? "",
                Phone = user.Phone
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            if (!ModelState.IsValid) return Page();

            var error = await _authService.UpdateProfileAsync(userId.Value, Input.FullName, Input.Email, Input.Phone);
            if (error != null) { ErrorMessage = error; return Page(); }

            // Refresh session
            HttpContext.Session.SetString("FullName", Input.FullName);
            HttpContext.Session.SetString("Email", Input.Email);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToPage("/Account/Profile");
        }
    }
}
