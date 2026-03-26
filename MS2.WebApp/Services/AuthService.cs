using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;
using BCrypt.Net;

namespace MS2.WebApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            if (user == null || !user.IsActive) return null;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<string?> RegisterAsync(string username, string password, string email, string fullName, string? phone)
        {
            if (await UsernameExistsAsync(username))
                return "Tên đăng nhập đã tồn tại.";

            if (await EmailExistsAsync(email))
                return "Email này đã được đăng ký.";

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                FullName = fullName,
                Phone = phone,
                Role = "Customer",
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return null; // success
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username);
            return user != null;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Any(u => u.Email != null && u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<string?> UpdateProfileAsync(int userId, string fullName, string email, string? phone)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return "Không tìm thấy người dùng.";

            // Check email uniqueness (exclude current user)
            var users = await _unitOfWork.Users.GetAllAsync();
            var emailOwner = users.FirstOrDefault(u => u.Email != null &&
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.Id != userId);
            if (emailOwner != null) return "Email này đã được sử dụng bởi tài khoản khác.";

            user.FullName = fullName;
            user.Email = email;
            user.Phone = phone;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return null;
        }

        public async Task<string?> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return "Không tìm thấy người dùng.";

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return "Mật khẩu hiện tại không đúng.";

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return null;
        }
    }
}
