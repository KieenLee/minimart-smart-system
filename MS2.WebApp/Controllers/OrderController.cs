using Microsoft.AspNetCore.Mvc;
using MS2.DataAccess.Interfaces;
using MS2.Models.Entities;
using MS2.WebApp.Models;
using System.Text.Json;

namespace MS2.WebApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "Cart";

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Order/Checkout" });
            }

            // Lấy giỏ hàng
            var cart = GetCart();
            if (cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin user để điền sẵn
            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);

            var model = new CheckoutViewModel
            {
                ReceiverName = user?.FullName ?? "",
                PhoneNumber = user?.Phone ?? "",
                CartItems = cart.Items,
                TotalAmount = cart.TotalAmount
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục!";
                return RedirectToAction("Login", "Account");
            }

            // Lấy giỏ hàng
            var cart = GetCart();
            if (cart.IsEmpty)
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Cập nhật cart items cho model để hiển thị lại khi có lỗi
            model.CartItems = cart.Items;
            model.TotalAmount = cart.TotalAmount;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra tồn kho
            foreach (var item in cart.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive)
                {
                    ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' không còn tồn tại.");
                    return View(model);
                }

                if (product.Stock < item.Quantity)
                {
                    ModelState.AddModelError("", $"Sản phẩm '{item.ProductName}' chỉ còn {product.Stock} trong kho.");
                    return View(model);
                }
            }

            // Tạo đơn hàng
            var order = new Order
            {
                CustomerId = userId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalAmount,
                Status = "Pending",
                OrderType = "Online",
                Notes = model.GetOrderNotes()
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Tạo order details và giảm stock
            foreach (var item in cart.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    // Tạo order detail
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        Subtotal = item.Subtotal
                    };

                    // Thêm vào DbContext trực tiếp
                    await _unitOfWork.Context.OrderDetails.AddAsync(orderDetail);

                    // Giảm stock
                    product.Stock -= item.Quantity;
                    await _unitOfWork.Products.UpdateAsync(product);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Xóa giỏ hàng
            HttpContext.Session.Remove(CartSessionKey);

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        // GET: Order/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null || order.CustomerId != userId.Value)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/History
        public async Task<IActionResult> History(int page = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Order/History" });
            }

            const int pageSize = 10;

            // Lấy đơn hàng của user
            var allOrders = (await _unitOfWork.Orders.GetAllAsync())
                .Where(o => o.CustomerId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var totalItems = allOrders.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var orders = allOrders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new OrderHistoryViewModel
            {
                Orders = orders,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(model);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(id);

            if (order == null || order.CustomerId != userId.Value)
            {
                return NotFound();
            }

            // Lấy order details từ Context
            var orderDetails = _unitOfWork.Context.OrderDetails
                .Where(od => od.OrderId == id)
                .ToList();

            // Load product cho mỗi order detail
            foreach (var detail in orderDetails)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    detail.Product = product;
                }
            }

            var model = new OrderDetailViewModel
            {
                Order = order,
                OrderDetails = orderDetails,
                TotalAmount = order.TotalAmount
            };

            return View(model);
        }

        #region Helper Methods

        private CartViewModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            return JsonSerializer.Deserialize<CartViewModel>(cartJson) ?? new CartViewModel();
        }

        #endregion
    }
}
