using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace MS2.DesktopApp.Models;

public partial class AdminOrdersViewModel : ObservableObject
{
    private readonly HubConnection _hubConnection;
    private List<OnlineOrderDto> _allOrders = new();

    [ObservableProperty]
    private ObservableCollection<OnlineOrderDto> orders = new();

    [ObservableProperty]
    private string selectedFilter = "Tất cả";

    public List<string> FilterOptions { get; } = new()
    {
        "Tất cả", "Pending", "Shipping", "Completed", "Cancelled"
    };

    [ObservableProperty]
    private string statusMessage = "Đang tải...";

    public AdminOrdersViewModel(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;

        // Lắng nghe real-time khi có đơn mới
        _hubConnection.On<int, string, decimal>("ReceiveNewOrder", (orderId, customer, total) =>
        {
            Application.Current.Dispatcher.Invoke(async () => await RefreshOrdersAsync());
        });

        // Lắng nghe real-time khi đơn được duyệt
        _hubConnection.On<int, string, int, DateTime?>("ReceiveOrderApproved", (orderId, status, empId, approvedAt) =>
        {
            Application.Current.Dispatcher.Invoke(async () => await RefreshOrdersAsync());
        });

        // Lắng nghe real-time khi trạng thái đổi (Completed từ Worker)
        _hubConnection.On<int, string>("ReceiveOrderStatusUpdate", (orderId, status) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var order = _allOrders.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.Status = status;
                    ApplyFilter();
                }
            });
        });
    }

    public async Task InitializeAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try { await _hubConnection.StartAsync(); }
            catch
            {
                MessageBox.Show("Không thể kết nối tới Server. Vui lòng đảm bảo WebApp đang chạy.");
                return;
            }
        }

        var timeout = DateTime.Now.AddSeconds(3);
        while (_hubConnection.State == HubConnectionState.Connecting && DateTime.Now < timeout)
            await Task.Delay(100);

        if (_hubConnection.State != HubConnectionState.Connected)
        {
            MessageBox.Show("Kết nối tới Server chưa sẵn sàng. Thử lại sau.");
            return;
        }

        await RefreshOrdersAsync();
    }

    [RelayCommand]
    private async Task RefreshOrdersAsync()
    {
        try
        {
            StatusMessage = "Đang tải đơn hàng...";
            var result = await _hubConnection.InvokeAsync<List<OnlineOrderDto>>("GetAllOnlineOrders");
            _allOrders = result ?? new();
            ApplyFilter();
            StatusMessage = $"Tổng: {_allOrders.Count} đơn Online";
        }
        catch (Exception ex)
        {
            StatusMessage = "Lỗi tải dữ liệu";
            MessageBox.Show($"Lỗi tải đơn hàng: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ShowOrderDetail(OnlineOrderDto? order)
    {
        if (order == null) return;
        var detailWindow = new MS2.DesktopApp.Presentation.Orders.OrderDetailWindow(order);
        detailWindow.Owner = System.Windows.Application.Current.MainWindow;
        detailWindow.ShowDialog();
    }

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = SelectedFilter == "Tất cả"
            ? _allOrders
            : _allOrders.Where(o => o.Status == SelectedFilter).ToList();

        Orders.Clear();
        foreach (var o in filtered) Orders.Add(o);
    }
}
