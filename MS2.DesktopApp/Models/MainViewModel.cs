using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MS2.DesktopApp.Network;
using MS2.DesktopApp.Presentation.POS;
using MS2.DesktopApp.Presentation.Inventory;
using MS2.DesktopApp.Presentation.Reports;
using MS2.DesktopApp.Presentation.Employees;
using MS2.DesktopApp.Presentation.Profile;
using MS2.DesktopApp.Presentation.Orders;
using MS2.Models.DTOs.Auth;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class MainViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly HubConnection _hubConnection;

    [ObservableProperty]
    private UserDto currentUser;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private Visibility isAdmin = Visibility.Collapsed;

    public MainViewModel(TcpClientService tcpClient, UserDto user, HubConnection hubConnection)
    {
        _tcpClient = tcpClient;
        CurrentUser = user;
        _hubConnection = hubConnection;

        // Set visibility for Admin-only buttons
        if (user.Role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
        {
            IsAdmin = Visibility.Visible;
        }

        // Lắng nghe sự kiện Đơn mới từ WebApp
        _hubConnection.On<int, string, decimal>("ReceiveNewOrder", (orderId, customer, total) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Có đơn hàng Online MỚI mã #{orderId} từ {customer}.\nTổng tiền: {total:N0}đ", "Thông báo Đơn Hàng Mới", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        });

        // Khởi động Background SignalR
        if (_hubConnection.State != HubConnectionState.Connected)
        {
            try { _hubConnection.StartAsync(); } catch { }
        }

        // Default view: Show welcome message
        CurrentView = CreateWelcomeView();
    }

    [RelayCommand]
    private async Task NavigateToPos()
    {
        try
        {
            var posViewModel = new PosViewModel(_tcpClient, CurrentUser, _hubConnection);
            var posView = new PosView { DataContext = posViewModel };
            CurrentView = posView;

            // Initialize async sau khi UI đã render
            await posViewModel.InitializeAsync();
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task NavigateToInventory()
    {
        try
        {
            var inventoryViewModel = new InventoryViewModel(_tcpClient, CurrentUser, _hubConnection);
            var inventoryView = new InventoryView { DataContext = inventoryViewModel };
            CurrentView = inventoryView;

            // Initialize async sau khi UI đã render
            await inventoryViewModel.InitializeAsync();
        }
        catch
        {
        }
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        var reportsViewModel = new ReportsViewModel(_tcpClient, CurrentUser);
        var reportsView = new ReportsView { DataContext = reportsViewModel };
        CurrentView = reportsView;
    }

    [RelayCommand]
    private async Task NavigateToEmployees()
    {
        try
        {
            var employeesViewModel = new EmployeesViewModel(_tcpClient, CurrentUser);
            var employeesView = new EmployeesView { DataContext = employeesViewModel };
            CurrentView = employeesView;

            // Initialize async sau khi UI đã render
            await employeesViewModel.InitializeAsync();
        }
        catch
        {
        }
    }

    [RelayCommand]
    private void NavigateToProfile()
    {
        var profileViewModel = new ProfileViewModel(_tcpClient, CurrentUser);
        var profileView = new ProfileView { DataContext = profileViewModel };
        CurrentView = profileView;
    }

    [RelayCommand]
    private async Task NavigateToOrders()
    {
        try
        {
            var ordersViewModel = new MS2.DesktopApp.Models.OrdersViewModel(_hubConnection, CurrentUser);
            var ordersView = new MS2.DesktopApp.Presentation.Orders.OrdersView { DataContext = ordersViewModel };
            CurrentView = ordersView;

            await ordersViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi mở danh sách Đơn hàng: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToAdminOrders()
    {
        try
        {
            var vm = new MS2.DesktopApp.Models.AdminOrdersViewModel(_hubConnection);
            var view = new MS2.DesktopApp.Presentation.Orders.AdminOrdersView { DataContext = vm };
            CurrentView = view;
            await vm.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi mở Quản lý Đơn Online: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Logout()
    {
        // Disconnect TCP
        _tcpClient.Disconnect();

        // Lấy LoginWindow từ DI Container và hiển thị lại
        var app = (App)Application.Current;
        var serviceProvider = app.ServiceProvider;

        if (serviceProvider != null)
        {
            var loginWindow = serviceProvider.GetRequiredService<MS2.DesktopApp.Presentation.LoginWindow>();
            var loginViewModel = serviceProvider.GetRequiredService<LoginViewModel>();
            loginViewModel.Username = "";
            loginViewModel.Password = "";
            loginViewModel.ErrorMessage = "";
            loginViewModel.ErrorVisibility = Visibility.Collapsed;
            loginWindow.DataContext = loginViewModel;
            loginWindow.Show();
        }

        // Đóng MainWindow hiện tại
        foreach (Window window in Application.Current.Windows)
        {
            if (window is MainWindow)
            {
                window.Close();
                break;
            }
        }
    }

    // Helper method to create welcome view
    private object CreateWelcomeView()
    {
        return new System.Windows.Controls.TextBlock
        {
            Text = $"Chào mừng {CurrentUser.FullName}!\n\nVui lòng chọn chức năng từ menu bên trái.",
            FontSize = 18,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
    }
}