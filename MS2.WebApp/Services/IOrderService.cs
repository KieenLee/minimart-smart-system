using MS2.Models.Entities;
using MS2.WebApp.Models;

namespace MS2.WebApp.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Place a new order. Validates stock availability, reduces stock, saves order & details.
        /// Returns (orderId, errorMessage) — if errorMessage != null, order failed.
        /// </summary>
        Task<(int orderId, string? error)> PlaceOrderAsync(int customerId, string receiverName, string phone, string address, string? note, List<CartItemViewModel> cartItems);

        /// <summary>Get all orders for a specific customer, newest first.</summary>
        Task<List<Order>> GetOrdersByCustomerAsync(int customerId);

        /// <summary>Get a single order with full details (OrderDetails + Products). Validates ownership.</summary>
        Task<Order?> GetOrderDetailsAsync(int orderId, int customerId);
    }
}
