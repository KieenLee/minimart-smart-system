using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MS2.Models.Entities;

namespace MS2.WebApp.Hubs
{
    // Hub là trung tâm nhận/phát tín hiệu Real-time
    public class MiniMartHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public MiniMartHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // 1. Báo hiệu đơn mới
        public async Task NotifyNewOrder(int orderId, string customerName, decimal totalAmount)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", orderId, customerName, totalAmount);
        }

        // 2. Báo hiệu trạng thái đơn đổi
        public async Task UpdateOrderStatus(int orderId, string status)
        {
            await Clients.All.SendAsync("ReceiveOrderStatusUpdate", orderId, status);
        }

        // 3. Báo hiệu tồn kho đổi
        public async Task UpdateStock(int productId, int remainingStock)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", productId, remainingStock);
        }

        // --- CHO DESKTOP EMP: Lấy đơn Pending (dùng DTO phẳng tránh Circular Reference) ---
        public async Task<List<object>> GetPendingOrders()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
            var orders = await db.Orders
                .Where(o => o.Status == "Pending" && o.OrderType == "Online")
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => (object)new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.Notes,
                    CustomerName = o.Customer != null ? o.Customer.FullName : ""
                })
                .ToListAsync();
            return orders;
        }

        // --- CHO DESKTOP ADMIN: Lấy tất cả đơn Online (dùng DTO phẳng tránh Circular Reference) ---
        public async Task<List<object>> GetAllOnlineOrders()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
            var orders = await db.Orders
                .Where(o => o.OrderType == "Online")
                .Include(o => o.Customer)
                .Include(o => o.ApprovedByEmployee)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => (object)new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.Notes,
                    o.ApprovedAt,
                    CustomerName    = o.Customer != null ? o.Customer.FullName : "",
                    ApproverName    = o.ApprovedByEmployee != null ? o.ApprovedByEmployee.FullName : "",
                    ApproverEmpId   = o.ApprovedByEmployeeId
                })
                .ToListAsync();
            return orders;
        }

        // --- CHO DESKTOP EMP: Duyệt đơn ---
        public async Task<bool> ApproveOrder(int orderId, int employeeId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MS2.DataAccess.Data.MS2DbContext>();
                var order = await db.Orders.FindAsync(orderId);
                if (order != null && order.Status == "Pending")
                {
                    order.Status = "Shipping";
                    order.ApprovedByEmployeeId = employeeId;
                    order.ApprovedAt = DateTime.Now;
                    await db.SaveChangesAsync();

                    // Broadcast kèm người duyệt, giờ duyệt
                    await Clients.All.SendAsync("ReceiveOrderApproved", orderId, "Shipping", employeeId, order.ApprovedAt);
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
