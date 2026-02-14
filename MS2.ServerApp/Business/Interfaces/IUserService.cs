using MS2.Models.TCP;

namespace MS2.ServerApp.Business.Interfaces;

public interface IUserService
{
    // Lấy danh sách tất cả nhân viên (active users)
    Task<TcpResponse> GetEmployeesAsync(TcpMessage message);

    // Lấy danh sách users theo role (Admin, Employee, Customer)
    Task<TcpResponse> GetUsersByRoleAsync(TcpMessage message);

    // Tìm kiếm users theo keyword (tên, username, email, phone)
    Task<TcpResponse> SearchUsersAsync(TcpMessage message);

    // Tạo user/nhân viên mới
    Task<TcpResponse> CreateUserAsync(TcpMessage message);

    // Cập nhật thông tin profile của user
    Task<TcpResponse> UpdateUserProfileAsync(TcpMessage message);
}
