using Microsoft.EntityFrameworkCore;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;
using MS2.WebApp.Models;

namespace MS2.WebApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(int orderId, string? error)> PlaceOrderAsync(
            int customerId, string receiverName, string phone,
            string address, string? note, List<CartItemViewModel> cartItems)
        {
            if (cartItems == null || cartItems.Count == 0)
                return (0, "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi đặt hàng.");

            // Validate stock for every item
            foreach (var item in cartItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null)
                    return (0, $"Sản phẩm '{item.ProductName}' không còn tồn tại.");
                if (product.IsActive != true)
                    return (0, $"Sản phẩm '{item.ProductName}' hiện không còn bán.");
                if (item.Quantity > product.Stock)
                    return (0, $"Sản phẩm '{item.ProductName}' chỉ còn {product.Stock} trong kho.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var notesText = $"Người nhận: {receiverName}\nSĐT: {phone}\nĐịa chỉ: {address}";
                if (!string.IsNullOrEmpty(note)) notesText += $"\nGhi chú: {note}";

                // Calculate total
                decimal totalAmount = cartItems.Sum(x => x.Subtotal);

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    Notes = notesText,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    OrderType = "Online"
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync(); // get order.Id

                foreach (var item in cartItems)
                {
                    // Optimistic Concurrency Control (OCC) using ExecuteUpdateAsync
                    var rowsAffected = await _unitOfWork.Context.Products
                        .Where(p => p.Id == item.ProductId && p.Stock >= item.Quantity)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Quantity));

                    if (rowsAffected == 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return (0, $"Rất tiếc, sản phẩm '{item.ProductName}' vừa có người mua xong nên không đủ số lượng để thanh toán.");
                    }

                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        Subtotal = item.Subtotal
                    };
                    await _unitOfWork.Context.OrderDetails.AddAsync(detail);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return (order.Id, null);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                return (0, "Đã xảy ra lỗi khi xử lý đơn hàng. Vui lòng thử lại.");
            }
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync();
            return orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public async Task<Order?> GetOrderDetailsAsync(int orderId, int customerId)
        {
            var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync();
            return orders.FirstOrDefault(o => o.Id == orderId && o.CustomerId == customerId);
        }
    }
}
