using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            if (HttpContext.Session.GetInt32("UserId").HasValue)
                return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var error = await _authService.RegisterAsync(
                Input.Username, Input.Password,
                Input.Email, Input.FullName, Input.PhoneNumber);

            if (error != null)
            {
                ErrorMessage = error;
                return Page();
            }

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToPage("/Account/Login");
        }
    }
}
