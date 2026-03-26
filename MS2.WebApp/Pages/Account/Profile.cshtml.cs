using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MS2.Models.Entities;
using MS2.WebApp.Services;

namespace MS2.WebApp.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        public ProfileModel(IAuthService authService) { _authService = authService; }

        public MS2.Models.Entities.User? CurrentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToPage("/Account/Login");

            CurrentUser = await _authService.GetUserByIdAsync(userId.Value);
            if (CurrentUser == null) return RedirectToPage("/Account/Login");
            return Page();
        }
    }
}
