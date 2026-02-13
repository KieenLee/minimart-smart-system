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
    private void NavigateToPos()
    {
        CurrentView = new PosView();
    }

    [RelayCommand]
    private void NavigateToInventory()
    {
        CurrentView = new InventoryView();
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentView = new ReportsView();
    }

    [RelayCommand]
    private void NavigateToEmployees()
    {
        CurrentView = new EmployeesView();
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