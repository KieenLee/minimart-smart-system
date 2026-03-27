using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.DTOs.Product;
using MS2.Models.TCP;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace MS2.DesktopApp.Models;

public partial class InventoryViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private string searchKeyword = "";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "";

    // Fields for editing
    [ObservableProperty]
    private decimal newPrice = 0;

    [ObservableProperty]
    private int newStock = 0;

    // Fields for creating new product
    [ObservableProperty]
    private List<MS2.Models.DTOs.Product.CategoryDto> categories = new();

    [ObservableProperty]
    private int selectedCategoryId = 0;

    private readonly HubConnection _hubConnection;

    public InventoryViewModel(TcpClientService tcpClient, UserDto currentUser, HubConnection hubConnection)
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
                    Products[index] = product; // Cập nhật lại Grid View
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
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khởi tạo: {ex.Message}";
            MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
                    foreach (var product in productList)
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
                    foreach (var product in productList)
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
    private void SelectProduct(ProductDto? product)
    {
        SelectedProduct = product;
        if (product != null)
        {
            NewPrice = product.Price;
            NewStock = product.Stock;
            StatusMessage = $"Đã chọn: {product.Name}";
        }
    }

    [RelayCommand]
    private async Task UpdatePriceAsync()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewPrice <= 0)
        {
            MessageBox.Show("Giá phải lớn hơn 0!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Cập nhật giá sản phẩm '{SelectedProduct.Name}'?\n\nGiá cũ: {SelectedProduct.Price:N0}đ\nGiá mới: {NewPrice:N0}đ",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Đang cập nhật giá...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.UPDATE_PRODUCT_PRICE,
                new { ProductId = SelectedProduct.Id, NewPrice = NewPrice },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show("Cập nhật giá thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Đã cập nhật giá thành công";

                // Reload products
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show($"Cập nhật giá thất bại!\n{response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateStockAsync()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewStock < 0)
        {
            MessageBox.Show("Tồn kho không được âm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Cập nhật tồn kho sản phẩm '{SelectedProduct.Name}'?\n\nTồn kho cũ: {SelectedProduct.Stock}\nTồn kho mới: {NewStock}",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result != MessageBoxResult.Yes) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Đang cập nhật tồn kho...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.UPDATE_PRODUCT_STOCK,
                new { ProductId = SelectedProduct.Id, NewStock = NewStock },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show("Cập nhật tồn kho thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "Đã cập nhật tồn kho thành công";

                // Reload products
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show($"Cập nhật tồn kho thất bại!\n{response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await _tcpClient.SendMessageAsync(
                TcpActions.GET_CATEGORIES, null, _tcpClient.CurrentSessionId);
            if (response?.Success == true)
            {
                var json = response.Data?.ToString() ?? "[]";
                var list = System.Text.Json.JsonSerializer.Deserialize<List<MS2.Models.DTOs.Product.CategoryDto>>(
                    json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Categories = list ?? new();
            }
        }
        catch { }
    }

    [RelayCommand]
    private void OpenCreateProductDialog()
    {
        _ = LoadCategoriesAsync();
        SelectedCategoryId = 0;
        var dialog = new MS2.DesktopApp.Presentation.Inventory.CreateProductWindow(this);

        // Tìm MainWindow chính xác theo kiểu để tránh lỗi "Owner to itself"
        var mainWindow = System.Windows.Application.Current.Windows
            .OfType<MainWindow>()
            .FirstOrDefault();
        if (mainWindow != null)
            dialog.Owner = mainWindow;

        dialog.ShowDialog();

        if (dialog.IsConfirmed)
            _ = DoCreateProductAsync(dialog);
    }

    private async Task DoCreateProductAsync(MS2.DesktopApp.Presentation.Inventory.CreateProductWindow dialog)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Đang tạo sản phẩm...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.CREATE_PRODUCT,
                new
                {
                    Name = dialog.ProductName,
                    CategoryId = SelectedCategoryId,
                    Price = dialog.Price,
                    Stock = dialog.Stock,
                    Barcode = dialog.Barcode
                },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show("✅ Tạo sản phẩm thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show($"Lỗi: {response?.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = $"Lỗi tạo SP: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void ExportTemplate()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Lưu file mẫu nhập kho",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = "MauNhapKho.xlsx",
            DefaultExt = "xlsx"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Sản phẩm");

            var headers = new[] { "Tên SP *", "Giá Bán (đ) *", "Số lượng NK", "Barcode", "Mô tả" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A2340");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Dòng mẫu
            ws.Cell(2, 1).Value = "Coca Cola 330ml";
            ws.Cell(2, 2).Value = 15000;
            ws.Cell(2, 3).Value = 50;
            ws.Cell(2, 4).Value = "COCA330";
            ws.Cell(2, 5).Value = "Nước ngọt có ga";

            ws.Columns().AdjustToContents();
            wb.SaveAs(dialog.FileName);

            MessageBox.Show($"✅ File mẫu đã lưu!\n{dialog.FileName}\n\nHướng dẫn:\n- Tên/Barcode đã có → cộng thêm số lượng\n- Tên/Barcode chưa có → thêm sản phẩm mới",
                "Xuất File Mẫu", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ImportProductsAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Chọn file Excel nhập kho",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            Multiselect = false
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Đang đọc file Excel...";

            var importRows = new System.Collections.Generic.List<object>();

            using var wb = new XLWorkbook(dialog.FileName);
            var ws = wb.Worksheets.First();
            int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

            for (int r = 2; r <= lastRow; r++)
            {
                var name = ws.Cell(r, 1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name)) continue;

                decimal.TryParse(ws.Cell(r, 2).GetString().Replace(",", ""), out var price);
                int.TryParse(ws.Cell(r, 3).GetString().Replace(",", ""), out var stock);
                var barcode = ws.Cell(r, 4).GetString().Trim();
                var description = ws.Cell(r, 5).GetString().Trim();

                importRows.Add(new { Name = name, Price = price, Stock = stock, Barcode = barcode, Description = description, CategoryId = 1 });
            }

            if (importRows.Count == 0)
            {
                MessageBox.Show("File Excel không có dữ liệu hợp lệ!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = $"Đang gửi {importRows.Count} dòng lên server...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.BATCH_UPSERT_PRODUCTS,
                importRows,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show($"✅ {response.Message}", "Import thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProductsAsync();
            }
            else
            {
                MessageBox.Show($"Lỗi: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi đọc file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "";
        }
    }
}
