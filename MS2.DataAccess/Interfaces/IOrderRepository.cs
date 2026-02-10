using MS2.Models.Entities;

namespace MS2.DataAccess.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Order>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<Order?> GetWithDetailsAsync(int orderId);
    Task<IEnumerable<Order>> GetByStatusAsync(string status);
    Task<IEnumerable<Order>> GetByOrderTypeAsync(string orderType);
    Task<decimal> GetTotalSalesByDateRangeAsync(DateTime fromDate, DateTime toDate);
}