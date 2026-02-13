using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.DesktopApp.Presentation.POS;
using MS2.DesktopApp.Presentation.Inventory;
using MS2.DesktopApp.Presentation.Reports;
using MS2.DesktopApp.Presentation.Employees;
using MS2.Models.DTOs.Auth;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class MainViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;

    [ObservableProperty]
    private UserDto currentUser;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private Visibility isAdmin = Visibility.Collapsed;

    public MainViewModel(TcpClientService tcpClient, UserDto user)
    {
        _tcpClient = tcpClient;
        CurrentUser = user;

        // Set visibility for Admin-only buttons
        if (user.Role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
        {
            IsAdmin = Visibility.Visible;
        }

        // Default view: Show welcome message
        CurrentView = CreateWelcomeView();
    }

    [RelayCommand]
    private async Task NavigateToPos()
    {
        try
        {
            var posViewModel = new PosViewModel(_tcpClient, CurrentUser);
            var posView = new PosView { DataContext = posViewModel };
            CurrentView = posView;

            // Initialize async sau khi UI đã render
            await posViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở POS: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task NavigateToInventory()
    {
        try
        {
            var inventoryViewModel = new InventoryViewModel(_tcpClient, CurrentUser);
            var inventoryView = new InventoryView { DataContext = inventoryViewModel };
            CurrentView = inventoryView;

            // Initialize async sau khi UI đã render
            await inventoryViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở Inventory: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi mở Employees: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var result = MessageBox.Show(
            "Bạn có chắc muốn đăng xuất?",
            "Xác nhận",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            // Disconnect TCP
            _tcpClient.Disconnect();

            // Close MainWindow and show LoginWindow
            Application.Current.Windows[0]?.Close();
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