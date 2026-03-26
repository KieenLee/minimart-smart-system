using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.WebApp.Models;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;

        public LoginModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

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

            var user = await _authService.LoginAsync(Input.Username, Input.Password);
            if (user == null)
            {
                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng. Vui lòng thử lại.";
                return Page();
            }

            // Set session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            TempData["Success"] = $"Chào mừng, {user.FullName ?? user.Username}!";
            return RedirectToPage("/Index");
        }
    }
}
