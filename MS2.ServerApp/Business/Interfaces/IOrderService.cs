using MS2.Models.TCP;

namespace MS2.ServerApp.Business.Interfaces
{
    public interface IOrderService
    {
        // Tạo đơn hàng mới (checkout)
        Task<TcpResponse> CreateOrderAsync(TcpMessage message);

        // Lấy danh sách đơn hàng
        Task<TcpResponse> GetOrdersAsync(TcpMessage message);

        // Lấy chi tiết đơn hàng
        Task<TcpResponse> GetOrderDetailsAsync(TcpMessage message);

        // Lấy báo cáo doanh thu theo khoảng thời gian
        Task<TcpResponse> GetSalesReportAsync(TcpMessage message);
    }
}