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
using Microsoft.AspNetCore.SignalR.Client;

namespace MS2.DesktopApp.Models;

public partial class PosViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;
    private readonly HubConnection _hubConnection;

    [ObservableProperty]
    private string searchKeyword = "";

    partial void OnSearchKeywordChanged(string value)
    {
        // Tự động tìm kiếm ngay sau khi gõ kí tự
        SearchProductsCommand.Execute(null);
    }

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> cartItems = new();

    [ObservableProperty]
    private decimal totalAmount = 0;

    [ObservableProperty]
    private bool isLoading = false;

    public PosViewModel(TcpClientService tcpClient, UserDto currentUser, HubConnection hubConnection)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;
        _hubConnection = hubConnection;

        // Bắt sự kiện Real-time trừ tồn kho từ Server
        _hubConnection.On<int, int>("ReceiveStockUpdate", (productId, stock) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var product = Products.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    var index = Products.IndexOf(product);
                    product.Stock = stock;
                    if (product.Stock <= 0)
                    {
                        Products.RemoveAt(index); // Ẩn khỏi Grid nếu hết hàng
                    }
                    else
                    {
                        Products[index] = product; // Gán chèn lại lên chính index đó để kích gõ CollectionChanged đến UI Grid WPF
                    }
                }

                // Cập nhật giỏ hàng nếu sản phẩm đang tồn tại trong giỏ
                var cartItem = CartItems.FirstOrDefault(c => c.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.MaxQuantity = stock;
                    if (cartItem.Quantity > stock)
                    {
                        cartItem.Quantity = stock; // Tự động kích hoạt Quantity setter để hạ số lượng xuống tồn kho tối đa
                    }
                }
            });
        });
    }

    /// <summary>
    /// Initialize async - gọi sau khi constructor xong
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await LoadProductsAsync();
        }
        catch
        {
            // Silent error handling
        }
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;

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
            }
        }
        catch
        {
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
            }
        }
        catch
        {
            // Silent error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddToCart(ProductDto? product)
    {
        try
        {
            if (product == null || product.Stock <= 0)
            {
                return;
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ chưa
            var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                // Kiểm tra số lượng tối đa
                if (existingItem.Quantity >= product.Stock)
                {
                    return;
                }

                // Tăng số lượng
                existingItem.Quantity++;
            }
            else
            {
                // Thêm mới
                CartItems.Add(new CartItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name ?? "Unknown",
                    UnitPrice = product.Price,
                    MaxQuantity = product.Stock,
                    Quantity = 1,
                    QuantityChangedCallback = UpdateTotalAmount
                });
            }

            UpdateTotalAmount();
        }
        catch
        {
            // Silent error
        }
    }

    [RelayCommand]
    private void AddToCartWithQuantity((ProductDto product, int quantity) data)
    {
        try
        {
            var (product, quantity) = data;

            if (product == null)
            {
                return;
            }

            if (quantity <= 0)
            {
                return;
            }

            if (product.Stock <= 0)
            {
                return;
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ chưa
            var existingItem = CartItems.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                // Kiểm tra số lượng tối đa
                var newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    return;
                }

                // Tăng số lượng theo input
                existingItem.Quantity += quantity;
            }
            else
            {
                // Kiểm tra số lượng không vượt quá tồn kho
                if (quantity > product.Stock)
                {
                    return;
                }

                // Thêm mới với số lượng
                CartItems.Add(new CartItemModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name ?? "Unknown",
                    UnitPrice = product.Price,
                    MaxQuantity = product.Stock,
                    Quantity = quantity,
                    QuantityChangedCallback = UpdateTotalAmount
                });

            }

            UpdateTotalAmount();
        }
        catch
        {
            // Silent error
        }
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemModel cartItem)
    {
        if (cartItem == null) return;

        CartItems.Remove(cartItem);
        UpdateTotalAmount();
    }

    [RelayCommand]
    private void ClearCart()
    {
        if (CartItems.Count == 0) return;

        CartItems.Clear();
        UpdateTotalAmount();
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (CartItems.Count == 0) return;

        try
        {
            IsLoading = true;

            // Tạo order DTO
            var createOrderDto = new CreateOrderDto
            {
                CustomerId = 1,
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
                // Clear giỏ hàng
                CartItems.Clear();
                UpdateTotalAmount();

                // Reload products để cập nhật stock
                await LoadProductsAsync();
            }
        }
        catch
        {
            // Silent error
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
