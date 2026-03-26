using MS2.Models.Entities;

namespace MS2.WebApp.Services
{
    public interface IAuthService
    {
        /// <summary>Validate credentials, return User if success, null if fail.</summary>
        Task<User?> LoginAsync(string username, string password);

        /// <summary>Register a new Customer. Returns error message or null on success.</summary>
        Task<string?> RegisterAsync(string username, string password, string email, string fullName, string? phone);

        /// <summary>Check if a username already exists.</summary>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>Check if an email already exists.</summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>Get user by ID.</summary>
        Task<User?> GetUserByIdAsync(int userId);

        /// <summary>Update profile info (fullname, email, phone).</summary>
        Task<string?> UpdateProfileAsync(int userId, string fullName, string email, string? phone);

        /// <summary>Change password. Returns error message or null on success.</summary>
        Task<string?> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
