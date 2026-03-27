using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MS2.WebApp.Hubs;
using MS2.DataAccess.Data; // Direct access since UnitOfWork might be scoped

namespace MS2.WebApp.Services
{
    public class OrderTimeoutWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MiniMartHub> _hubContext;

        public OrderTimeoutWorker(IServiceScopeFactory scopeFactory, IHubContext<MiniMartHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<MS2DbContext>();

                    var thresholdTime = DateTime.Now.AddMinutes(-30);
                    
                    var autoCompletingOrders = await dbContext.Orders
                        .Where(o => o.Status == "Shipping" && o.OrderDate <= thresholdTime)
                        .ToListAsync(stoppingToken);

                    foreach (var order in autoCompletingOrders)
                    {
                        order.Status = "Completed";
                        
                        // Thông báo về Web (trang Lịch sử) và DesktopApp bằng SignalR
                        await _hubContext.Clients.All.SendAsync("ReceiveOrderStatusUpdate", order.Id, "Completed");
                    }

                    if (autoCompletingOrders.Any())
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception)
                {
                    // Ignore transient errors
                }

                // Chạy quét lặp lại mỗi 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
