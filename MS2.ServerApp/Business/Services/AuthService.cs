using MS2.DataAccess.Interfaces;
using MS2.Models.DTOs.Auth;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;
using MS2.ServerApp.Models;
using System.Text.Json;

namespace MS2.ServerApp.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionManager _sessionManager;

        public AuthService(IUnitOfWork unitOfWork, ISessionManager sessionManager)
        {
            _unitOfWork = unitOfWork;
            _sessionManager = sessionManager;
        }

        public async Task<TcpResponse> LoginAsync(TcpMessage message)
        {
            try
            {
                // Deserialize LoginRequestDto
                var loginRequest = JsonSerializer.Deserialize<LoginRequestDto>(
                    JsonSerializer.Serialize(message.Data));

                if (loginRequest == null || string.IsNullOrWhiteSpace(loginRequest.Username))
                {
                    return TcpResponse.CreateError("Invalid login data", message.RequestId);
                }

                // Tìm user theo username
                var user = await _unitOfWork.Users.GetByUsernameAsync(loginRequest.Username);

                if (user == null)
                {
                    return TcpResponse.CreateError("User not found", message.RequestId);
                }

                // Verify password với BCrypt
                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    return TcpResponse.CreateError("Invalid password", message.RequestId);
                }

                // Tạo UserDto
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Address = user.Address,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                // Tạo session
                var session = new UserSession
                {
                    User = userDto
                };

                string sessionId = _sessionManager.CreateSession(session);

                // Return LoginResponseDto
                var response = new LoginResponseDto
                {
                    SessionId = sessionId,
                    User = userDto
                };

                return TcpResponse.CreateSuccess(response, "Login successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Login error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> RegisterAsync(TcpMessage message)
        {
            try
            {
                var registerRequest = JsonSerializer.Deserialize<RegisterRequestDto>(
                    JsonSerializer.Serialize(message.Data));

                if (registerRequest == null)
                {
                    return TcpResponse.CreateError("Invalid register data", message.RequestId);
                }

                // Kiểm tra username đã tồn tại
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(registerRequest.Username);
                if (existingUser != null)
                {
                    return TcpResponse.CreateError("Username already exists", message.RequestId);
                }

                // Hash password
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

                // Tạo user mới
                var newUser = new MS2.Models.Entities.User
                {
                    Username = registerRequest.Username,
                    PasswordHash = passwordHash,
                    FullName = registerRequest.FullName,
                    Email = registerRequest.Email,
                    Phone = registerRequest.Phone,
                    Role = registerRequest.Role ?? "Employee",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    Phone = newUser.Phone,
                    Role = newUser.Role,
                    IsActive = newUser.IsActive
                };

                return TcpResponse.CreateSuccess(userDto, "Register successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Register error: {ex.Message}", message.RequestId);
            }
        }

        public Task<TcpResponse> LogoutAsync(TcpMessage message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return Task.FromResult(TcpResponse.CreateError("Invalid session", message.RequestId));
                }

                bool removed = _sessionManager.RemoveSession(message.SessionId);

                if (removed)
                {
                    return Task.FromResult(TcpResponse.CreateSuccess(null, "Logout successful", message.RequestId));
                }
                else
                {
                    return Task.FromResult(TcpResponse.CreateError("Session not found", message.RequestId));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(TcpResponse.CreateError($"Logout error: {ex.Message}", message.RequestId));
            }
        }
    }
}