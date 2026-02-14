using MS2.DataAccess.Interfaces;
using MS2.Models.DTOs.Auth;
using MS2.Models.Entities;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;
using System.Text.Json;

namespace MS2.ServerApp.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionManager _sessionManager;

        public UserService(IUnitOfWork unitOfWork, ISessionManager sessionManager)
        {
            _unitOfWork = unitOfWork;
            _sessionManager = sessionManager;
        }

        public async Task<TcpResponse> GetEmployeesAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return TcpResponse.CreateError("Session required", message.RequestId);
                }

                var session = _sessionManager.GetSession(message.SessionId);
                if (session == null)
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                // Check if user is Admin (only Admin can view employees)
                if (!session.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Access denied. Admin only.", message.RequestId);
                }

                // Get only Employees
                var users = await _unitOfWork.Users.GetActiveUsersAsync();

                // Filter: Chỉ lấy Employee và Admin
                var employees = users.Where(u =>
                    u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                ).ToList();

                // Map to UserDto
                var userDtos = employees.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return TcpResponse.CreateSuccess(userDtos, $"Retrieved {userDtos.Count} users", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Error getting employees: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetUsersByRoleAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return TcpResponse.CreateError("Session required", message.RequestId);
                }

                var session = _sessionManager.GetSession(message.SessionId);
                if (session == null)
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                // Check if user is Admin
                if (!session.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Access denied. Admin only.", message.RequestId);
                }

                // Deserialize request data to get role
                var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(message.Data));

                if (requestData == null || !requestData.ContainsKey("Role"))
                {
                    return TcpResponse.CreateError("Role parameter required", message.RequestId);
                }

                string role = requestData["Role"];

                // Get users by role
                var users = await _unitOfWork.Users.GetByRoleAsync(role);

                // Map to UserDto
                var userDtos = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return TcpResponse.CreateSuccess(userDtos, $"Retrieved {userDtos.Count} users with role '{role}'", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Error getting users by role: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> SearchUsersAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return TcpResponse.CreateError("Session required", message.RequestId);
                }

                var session = _sessionManager.GetSession(message.SessionId);
                if (session == null)
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                // Check if user is Admin
                if (!session.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Access denied. Admin only.", message.RequestId);
                }

                // Deserialize request data to get search keyword
                var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    JsonSerializer.Serialize(message.Data));

                if (requestData == null || !requestData.ContainsKey("Keyword"))
                {
                    return TcpResponse.CreateError("Search keyword required", message.RequestId);
                }

                string keyword = requestData["Keyword"].Trim().ToLower();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    // If empty keyword, return all employees
                    return await GetEmployeesAsync(message);
                }

                // Get all active users
                var users = await _unitOfWork.Users.GetActiveUsersAsync();

                // Search in Employee role only
                var searchResults = users.Where(u =>
                    u.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase) &&
                    (u.FullName.ToLower().Contains(keyword) ||
                     u.Username.ToLower().Contains(keyword) ||
                     (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                     (u.Phone != null && u.Phone.Contains(keyword)))
                ).ToList();

                // Map to UserDto
                var userDtos = searchResults.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return TcpResponse.CreateSuccess(userDtos, $"Found {userDtos.Count} user(s)", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Error searching users: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> CreateUserAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return TcpResponse.CreateError("Session required", message.RequestId);
                }

                var session = _sessionManager.GetSession(message.SessionId);
                if (session == null)
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                // Check if user is Admin
                if (!session.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Access denied. Admin only.", message.RequestId);
                }

                // Deserialize CreateUserDto
                var createUserDto = JsonSerializer.Deserialize<CreateUserDto>(
                    JsonSerializer.Serialize(message.Data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (createUserDto == null)
                {
                    return TcpResponse.CreateError("Invalid user data", message.RequestId);
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(createUserDto.Username))
                {
                    return TcpResponse.CreateError("Username is required", message.RequestId);
                }

                if (string.IsNullOrWhiteSpace(createUserDto.Password))
                {
                    return TcpResponse.CreateError("Password is required", message.RequestId);
                }

                if (string.IsNullOrWhiteSpace(createUserDto.FullName))
                {
                    return TcpResponse.CreateError("Full name is required", message.RequestId);
                }

                if (string.IsNullOrWhiteSpace(createUserDto.Email))
                {
                    return TcpResponse.CreateError("Email is required", message.RequestId);
                }

                // Check if username already exists
                if (await _unitOfWork.Users.UsernameExistsAsync(createUserDto.Username))
                {
                    return TcpResponse.CreateError("Username already exists", message.RequestId);
                }

                // Check if email already exists
                if (await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email))
                {
                    return TcpResponse.CreateError("Email already exists", message.RequestId);
                }

                // Validate role (only allow Employee and Admin)
                if (!createUserDto.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase) &&
                    !createUserDto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Invalid role. Only Employee and Admin are allowed.", message.RequestId);
                }

                // Hash password with BCrypt (same as login)
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

                // Create new user entity
                var newUser = new User
                {
                    Username = createUserDto.Username,
                    PasswordHash = hashedPassword,
                    FullName = createUserDto.FullName,
                    Email = createUserDto.Email,
                    Phone = createUserDto.Phone,
                    Address = createUserDto.Address,
                    Role = createUserDto.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to database
                await _unitOfWork.Users.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                // Map to UserDto for response
                var userDto = new UserDto
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    FullName = newUser.FullName,
                    Email = newUser.Email,
                    Phone = newUser.Phone,
                    Address = newUser.Address,
                    Role = newUser.Role,
                    IsActive = newUser.IsActive,
                    CreatedAt = newUser.CreatedAt
                };

                return TcpResponse.CreateSuccess(userDto, "User created successfully", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Error creating user: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> UpdateUserProfileAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (string.IsNullOrWhiteSpace(message.SessionId))
                {
                    return TcpResponse.CreateError("Session required", message.RequestId);
                }

                var session = _sessionManager.GetSession(message.SessionId);
                if (session == null)
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                // Deserialize request
                var updateUserDto = JsonSerializer.Deserialize<UpdateUserDto>(
                    JsonSerializer.Serialize(message.Data),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (updateUserDto == null)
                {
                    return TcpResponse.CreateError("Invalid update data", message.RequestId);
                }

                // Security: User can only update their own profile unless they are Admin
                if (updateUserDto.UserId != session.User.Id &&
                    !session.User.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return TcpResponse.CreateError("Access denied. You can only update your own profile.", message.RequestId);
                }

                // Get user from database
                var user = await _unitOfWork.Users.GetByIdAsync(updateUserDto.UserId);
                if (user == null)
                {
                    return TcpResponse.CreateError("User not found", message.RequestId);
                }

                // Update user fields
                user.FullName = updateUserDto.FullName;
                user.Email = updateUserDto.Email;
                user.Phone = updateUserDto.Phone;
                user.Address = updateUserDto.Address;

                // Update password if provided
                if (!string.IsNullOrWhiteSpace(updateUserDto.NewPassword))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.NewPassword);
                }

                // Save changes
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Update session if user updated their own profile
                if (updateUserDto.UserId == session.User.Id)
                {
                    session.User.FullName = user.FullName;
                    session.User.Email = user.Email;
                    session.User.Phone = user.Phone;
                    session.User.Address = user.Address;
                }

                // Map to UserDto for response
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

                return TcpResponse.CreateSuccess(userDto, "Profile updated successfully", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Error updating profile: {ex.Message}", message.RequestId);
            }
        }
    }
}
