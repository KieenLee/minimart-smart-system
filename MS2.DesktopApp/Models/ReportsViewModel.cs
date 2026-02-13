using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.DTOs.Order;
using MS2.Models.TCP;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class ReportsViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now.AddDays(-7);

    [ObservableProperty]
    private DateTime toDate = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<OrderDto> orders = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "";

    [ObservableProperty]
    private decimal totalRevenue = 0;

    [ObservableProperty]
    private int totalOrders = 0;

    [ObservableProperty]
    private decimal averageOrderValue = 0;

    public ReportsViewModel(TcpClientService tcpClient, UserDto currentUser)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;
    }

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Đang tải báo cáo...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.GET_SALES_REPORT,
                new { FromDate = FromDate, ToDate = ToDate },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                var jsonString = response.Data?.ToString() ?? "{}";
                var report = JsonSerializer.Deserialize<SalesReportDto>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Orders.Clear();
                if (report != null)
                {
                    TotalRevenue = report.TotalRevenue;
                    TotalOrders = report.TotalOrders;
                    AverageOrderValue = report.AverageOrderValue;

                    if (report.Orders != null)
                    {
                        foreach (var order in report.Orders)
                        {
                            Orders.Add(order);
                        }
                    }
                }

                StatusMessage = $"Đã tải {Orders.Count} đơn hàng";
            }
            else
            {
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
