using Microsoft.AspNetCore.Mvc;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;
using MS2.WebApp.Models;
using BCrypt.Net;

namespace MS2.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ===== HELPER METHODS - SESSION =====
        private void SetUserSession(User user)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private bool IsLoggedIn()
        {
            return GetCurrentUserId() != null;
        }

        // ===== LOGIN =====
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Tìm user theo username
                var user = await _unitOfWork.Users.GetByUsernameAsync(model.Username);

                if (user == null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Kiểm tra password
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                    return View(model);
                }

                // Kiểm tra account có active không
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Tài khoản đã bị vô hiệu hóa");
                    return View(model);
                }

                // Chỉ cho phép Customer login vào Web
                if (user.Role != "Customer")
                {
                    ModelState.AddModelError("", "Bạn không có quyền truy cập trang này");
                    return View(model);
                }

                // Lưu session
                SetUserSession(user);

                // Redirect
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View(model);
            }
        }

        // ===== REGISTER =====
        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra username đã tồn tại chưa
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(model);
                }

                // Tạo user mới
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FullName = model.FullName,
                    Phone = model.PhoneNumber,
                    Role = "Customer", // Mặc định là Customer
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
                return View(model);
            }
        }

        // ===== LOGOUT =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ===== PROFILE =====
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });
            }

            var userId = GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(userId!.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // ===== EDIT PROFILE =====
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", new { returnUrl = "/Account/EditProfile" });
            }

            var userId = GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(userId!.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(userId!.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            // Cập nhật thông tin
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật session
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        // ===== CHANGE PASSWORD =====
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", new { returnUrl = "/Account/ChangePassword" });
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = GetCurrentUserId();
            var user = await _unitOfWork.Users.GetByIdAsync(userId!.Value);

            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            // Kiểm tra mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }
    }
}

