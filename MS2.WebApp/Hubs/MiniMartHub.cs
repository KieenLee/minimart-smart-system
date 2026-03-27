using Microsoft.AspNetCore.SignalR;
using MS2.Models.Entities;

namespace MS2.WebApp.Hubs
{
    // Hub là trung tâm nhận/phát tín hiệu Real-time
    // Code Hub sẽ được giữ siêu đơn giản để dễ bảo trì
    public class MiniMartHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public MiniMartHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        // 1. Gửi tín hiệu khi có người tạo hóa đơn mới
        public async Task NotifyNewOrder(int orderId, string customerName, decimal totalAmount)
        {
            // Phát cho tất cả Client (đặc biệt là DesktopApp của Nhân viên)
            await Clients.All.SendAsync("ReceiveNewOrder", orderId, customerName, totalAmount);
        }

        // 2. Gửi tín hiệu khi Nhân viên đổi trạng thái đơn hàng (VD: Pending -> Shipping)
        public async Task UpdateOrderStatus(int orderId, string status)
        {
            // Phát cho tất cả Client (Khách hàng đang xem Lịch sử đơn)
            await Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, status);
        }

        // 3. Gửi tín hiệu khi Tồn kho (Stock) của 1 sản phẩm thay đổi
        public async Task UpdateStock(int productId, int remainingStock)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", productId, remainingStock);
        }

        // --- CHO DESKTOP APP GỌI TRỰC TIẾP QUA SIGNALR ---
        public async Task<List<Order>> GetPendingOrders()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                System.Linq.Queryable.OrderByDescending(
                    System.Linq.Queryable.Where(db.Orders, o => o.Status == "Pending"), 
                    o => o.OrderDate));
        }

        public async Task<bool> ApproveOrder(int orderId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
                var order = await db.Orders.FindAsync(orderId);
                if (order != null && order.Status == "Pending")
                {
                    order.Status = "Shipping";
                    await db.SaveChangesAsync();
                    
                    // Bắn ngược lại signal cho Khách hàng
                    await UpdateOrderStatus(orderId, "Shipping");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("D:\\Workspace\\Project_.Net\\minimart-smart-system\\error_log.txt", ex.ToString());
                throw;
            }
        }
    }
}
