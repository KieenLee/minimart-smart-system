using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Data;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;

namespace MS2.DataAccess.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(MS2DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _dbSet
            .Where(o => o.EmployeeId == employeeId)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetWithDetailsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByOrderTypeAsync(string orderType)
    {
        return await _dbSet
            .Where(o => o.OrderType == orderType)
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSalesByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(o => o.OrderDate >= fromDate &&
                       o.OrderDate <= toDate &&
                       o.Status == "Completed")
            .SumAsync(o => o.TotalAmount);
    }
}