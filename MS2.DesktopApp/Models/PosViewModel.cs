using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.DTOs.Order;
using MS2.Models.DTOs.Product;
using MS2.Models.TCP;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class PosViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private string searchKeyword = "";

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> cartItems = new();

    [ObservableProperty]
    private decimal totalAmount = 0;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "";

    public PosViewModel(TcpClientService tcpClient, UserDto currentUser)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;

        // Load all products khi khởi tạo
        _ = LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Đang tải sản phẩm...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.GET_PRODUCTS,
                null,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                var jsonString = response.Data?.ToString() ?? "[]";
                var productList = JsonSerializer.Deserialize<List<ProductDto>>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Products.Clear();
                if (productList != null)
                {
                    foreach (var product in productList.Where(p => p.IsActive && p.Stock > 0))
                    {
                        Products.Add(product);
                    }
                }

                StatusMessage = $"Đã tải {Products.Count} sản phẩm";
            }
            else
            {
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchProductsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            await LoadProductsAsync();
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "Đang tìm kiếm...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.SEARCH_PRODUCTS,
                new { Keyword = SearchKeyword },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                var jsonString = response.Data?.ToString() ?? "[]";
                var productList = JsonSerializer.Deserialize<List<ProductDto>>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Products.Clear();
                if (productList != null)
                {
                    foreach (var product in productList.Where(p => p.IsActive && p.Stock > 0))
                    {
                        Products.Add(product);
                    }
                }

                StatusMessage = $"Tìm thấy {Products.Count} sản phẩm";
            }
            else
            {
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddToCart(ProductDto product)
    {
        if (product == null) return;

        // Kiểm tra xem sản phẩm đã có trong giỏ chưa
        var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem != null)
        {
            // Tăng số lượng
            existingItem.Quantity++;
        }
        else
        {
            // Thêm mới
            CartItems.Add(new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }

        UpdateTotalAmount();
        StatusMessage = $"Đã thêm {product.Name} vào giỏ hàng";
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemModel cartItem)
    {
        if (cartItem == null) return;

        CartItems.Remove(cartItem);
        UpdateTotalAmount();
        StatusMessage = $"Đã xóa {cartItem.ProductName} khỏi giỏ hàng";
    }

    [RelayCommand]
    private void ClearCart()
    {
        if (CartItems.Count == 0) return;

        var result = MessageBox.Show(
            "Bạn có chắc muốn xóa tất cả sản phẩm trong giỏ hàng?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            CartItems.Clear();
            UpdateTotalAmount();
            StatusMessage = "Đã xóa giỏ hàng";
        }
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0)
        {
            MessageBox.Show("Giỏ hàng trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Tổng tiền: {TotalAmount:N0}đ\n\nXác nhận thanh toán?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Đang xử lý đơn hàng...";

            // Tạo order DTO
            var createOrderDto = new CreateOrderDto
            {
                CustomerId = 1, // Default customer (có thể cải tiến sau để chọn khách hàng)
                EmployeeId = _currentUser.Id,
                OrderType = "POS",
                Notes = $"Thanh toán bởi {_currentUser.FullName}",
                OrderDetails = CartItems.Select(item => new CreateOrderDetailDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.CREATE_ORDER,
                createOrderDto,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show(
                    $"Thanh toán thành công!\nTổng tiền: {TotalAmount:N0}đ",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Clear giỏ hàng
                CartItems.Clear();
                UpdateTotalAmount();
                StatusMessage = "Đơn hàng đã được tạo thành công";

                // Reload products để cập nhật stock
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show(
                    $"Thanh toán thất bại!\n{response?.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Lỗi: {ex.Message}",
                "Lỗi",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateTotalAmount()
    {
        TotalAmount = CartItems.Sum(x => x.Subtotal);
    }
}
