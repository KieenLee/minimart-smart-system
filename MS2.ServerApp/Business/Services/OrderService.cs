using MS2.DataAccess.Interfaces;
using MS2.Models.DTOs.Order;
using MS2.Models.Entities;
using MS2.Models.TCP;
using MS2.ServerApp.Business.Interfaces;
using System.Text.Json;

namespace MS2.ServerApp.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionManager _sessionManager;

        public OrderService(IUnitOfWork unitOfWork, ISessionManager sessionManager)
        {
            _unitOfWork = unitOfWork;
            _sessionManager = sessionManager;
        }

        public async Task<TcpResponse> CreateOrderAsync(TcpMessage message)
        {
            try
            {
                // Validate session
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var createOrderDto = JsonSerializer.Deserialize<CreateOrderDto>(
                    JsonSerializer.Serialize(message.Data));

                if (createOrderDto == null || createOrderDto.OrderDetails.Count == 0)
                {
                    return TcpResponse.CreateError("Invalid order data", message.RequestId);
                }

                // Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Tạo Order
                    var order = new Order
                    {
                        CustomerId = createOrderDto.CustomerId,
                        EmployeeId = createOrderDto.EmployeeId,
                        OrderDate = DateTime.Now,
                        TotalAmount = 0, // Sẽ tính sau
                        Status = "Completed",
                        OrderType = createOrderDto.OrderType,
                        Notes = createOrderDto.Notes
                    };

                    await _unitOfWork.Orders.AddAsync(order);
                    await _unitOfWork.SaveAsync(); // Lưu để có OrderId

                    decimal totalAmount = 0;

                    // Tạo OrderDetails và cập nhật stock
                    foreach (var detail in createOrderDto.OrderDetails)
                    {
                        var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);

                        if (product == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return TcpResponse.CreateError($"Product ID {detail.ProductId} not found", message.RequestId);
                        }

                        // Kiểm tra stock
                        if (product.Stock < detail.Quantity)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return TcpResponse.CreateError(
                                $"Insufficient stock for {product.ProductName}. Available: {product.Stock}",
                                message.RequestId);
                        }

                        // Tạo OrderDetail
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice ?? product.Price,
                            Discount = detail.Discount ?? 0
                        };

                        decimal lineTotal = (orderDetail.UnitPrice * orderDetail.Quantity) - orderDetail.Discount;
                        totalAmount += lineTotal;

                        await _unitOfWork.Orders.AddOrderDetailAsync(orderDetail);

                        // Cập nhật stock
                        product.Stock -= detail.Quantity;
                        _unitOfWork.Products.Update(product);
                    }

                    // Cập nhật TotalAmount
                    order.TotalAmount = totalAmount;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveAsync();

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync();

                    // Lấy order với details để return
                    var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.OrderId);

                    var orderDto = new OrderDto
                    {
                        OrderId = createdOrder.OrderId,
                        CustomerId = createdOrder.CustomerId,
                        EmployeeId = createdOrder.EmployeeId,
                        OrderDate = createdOrder.OrderDate,
                        TotalAmount = createdOrder.TotalAmount,
                        Status = createdOrder.Status,
                        OrderType = createdOrder.OrderType,
                        Notes = createdOrder.Notes,
                        OrderDetails = createdOrder.OrderDetails.Select(od => new OrderDetailDto
                        {
                            OrderDetailId = od.OrderDetailId,
                            OrderId = od.OrderId,
                            ProductId = od.ProductId,
                            ProductName = od.Product?.ProductName,
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice,
                            Discount = od.Discount
                        }).ToList()
                    };

                    return TcpResponse.CreateSuccess(orderDto, "Order created successfully", message.RequestId);
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Create order error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetOrdersAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync();

                var orderDtos = orders.Select(o => new OrderDto
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    EmployeeId = o.EmployeeId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderType = o.OrderType,
                    Notes = o.Notes,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        Discount = od.Discount
                    }).ToList()
                }).ToList();

                return TcpResponse.CreateSuccess(orderDtos, "Get orders successful", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get orders error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetOrderDetailsAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var orderData = JsonSerializer.Deserialize<Dictionary<string, int>>(
                    JsonSerializer.Serialize(message.Data));

                int orderId = orderData?["OrderId"] ?? 0;

                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);

                if (order == null)
                {
                    return TcpResponse.CreateError("Order not found", message.RequestId);
                }

                var orderDto = new OrderDto
                {
                    OrderId = order.OrderId,
                    CustomerId = order.CustomerId,
                    EmployeeId = order.EmployeeId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    OrderType = order.OrderType,
                    Notes = order.Notes,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        Discount = od.Discount
                    }).ToList()
                };

                return TcpResponse.CreateSuccess(orderDto, "Order details retrieved", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get order details error: {ex.Message}", message.RequestId);
            }
        }

        public async Task<TcpResponse> GetSalesReportAsync(TcpMessage message)
        {
            try
            {
                if (!_sessionManager.IsValidSession(message.SessionId))
                {
                    return TcpResponse.CreateError("Invalid session", message.RequestId);
                }

                var reportData = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(
                    JsonSerializer.Serialize(message.Data));

                DateTime fromDate = reportData?["FromDate"] ?? DateTime.Now.AddMonths(-1);
                DateTime toDate = reportData?["ToDate"] ?? DateTime.Now;

                var salesReport = await _unitOfWork.Orders.GetSalesReportAsync(fromDate, toDate);

                return TcpResponse.CreateSuccess(salesReport, "Sales report retrieved", message.RequestId);
            }
            catch (Exception ex)
            {
                return TcpResponse.CreateError($"Get sales report error: {ex.Message}", message.RequestId);
            }
        }
    }
}