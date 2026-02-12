using MS2.Models.TCP;

namespace MS2.ServerApp.Business.Interfaces
{
    public interface IAuthService
    {
        // Xử lý đăng nhập
        Task<TcpResponse> LoginAsync(TcpMessage message);

        // Xử lý đăng ký user mới
        Task<TcpResponse> RegisterAsync(TcpMessage message);

        // Xử lý đăng xuất
        Task<TcpResponse> LogoutAsync(TcpMessage message);
    }
}