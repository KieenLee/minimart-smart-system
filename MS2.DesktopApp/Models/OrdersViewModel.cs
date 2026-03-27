using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using MS2.Models.DTOs.Auth;

namespace MS2.DesktopApp.Models;

public partial class OrdersViewModel : ObservableObject
{
    private readonly HubConnection _hubConnection;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private ObservableCollection<OnlineOrderDto> pendingOrders = new();

    public OrdersViewModel(HubConnection hubConnection, UserDto currentUser)
    {
        _hubConnection = hubConnection;
        _currentUser = currentUser;
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

        try
        {
            var orders = await _hubConnection.InvokeAsync<List<OnlineOrderDto>>("GetPendingOrders");
            Application.Current.Dispatcher.Invoke(() =>
            {
                PendingOrders.Clear();
                foreach (var o in orders ?? new()) PendingOrders.Add(o);
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
            var success = await _hubConnection.InvokeAsync<bool>("ApproveOrder", orderId, _currentUser.Id);
            if (success)
            {
                MessageBox.Show("✅ Đơn hàng đã được chuyển sang giao hàng.");
                await InitializeAsync();
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
