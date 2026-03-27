using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using MS2.Models.Entities;

namespace MS2.DesktopApp.Models;

public partial class OrdersViewModel : ObservableObject
{
    private readonly HubConnection _hubConnection;

    [ObservableProperty]
    private ObservableCollection<Order> pendingOrders = new();

    public OrdersViewModel(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    public async Task InitializeAsync()
    {
        // Chờ kết nối đến Hub nếu chưa Active
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch
            {
                MessageBox.Show("Không thể kết nối tới Server. Vui lòng đảm bảo WebApp đang chạy.");
                return;
            }
        }

        // Nếu đang Connecting thì đợi thêm tối đa 3 giây
        var timeout = DateTime.Now.AddSeconds(3);
        while (_hubConnection.State == HubConnectionState.Connecting && DateTime.Now < timeout)
        {
            await Task.Delay(100);
        }

        if (_hubConnection.State != HubConnectionState.Connected)
        {
            MessageBox.Show("Kết nối tới Server chưa sẵn sàng. Thử lại sau.");
            return;
        }

        try
        {
            // Yêu cầu danh sách đơn từ WebApp Hub
            var orders = await _hubConnection.InvokeAsync<List<Order>>("GetPendingOrders");
            Application.Current.Dispatcher.Invoke(() =>
            {
                PendingOrders.Clear();
                foreach (var o in orders) PendingOrders.Add(o);
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải đơn hàng: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ApproveOrder(int orderId)
    {
        try
        {
            var success = await _hubConnection.InvokeAsync<bool>("ApproveOrder", orderId);
            if (success)
            {
                MessageBox.Show("Thành công! Đơn hàng đã được chuyển sang giao hàng.");
                await InitializeAsync(); // Tải lại danh sách
            }
            else
            {
                MessageBox.Show("Duyệt đơn thất bại (Có thể đơn đã bị hủy hoặc duyệt rồi).");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi: {ex.Message}");
        }
    }
}
