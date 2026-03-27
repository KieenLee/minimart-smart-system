using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace MS2.DesktopApp.Models;

public partial class ReportsViewModel : ObservableObject
{
    private readonly Network.TcpClientService _tcpClient;
    private readonly MS2.Models.DTOs.Auth.UserDto _currentUser;
    private DispatcherTimer? _autoRefreshTimer;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private DateTime toDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<MS2.Models.DTOs.Order.OrderDto> orders = new();

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

    [ObservableProperty]
    private string lastUpdated = "";

    public ReportsViewModel(Network.TcpClientService tcpClient, MS2.Models.DTOs.Auth.UserDto currentUser)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;
    }

    public async Task InitializeAsync()
    {
        await LoadReportAsync();

        // Auto-refresh mỗi 30 giây
        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _autoRefreshTimer.Tick += async (s, e) => await LoadReportAsync();
        _autoRefreshTimer.Start();
    }

    public void Cleanup() => _autoRefreshTimer?.Stop();

    // Gọi lại khi ngày thay đổi
    partial void OnFromDateChanged(DateTime value) => _ = LoadReportAsync();
    partial void OnToDateChanged(DateTime value) => _ = LoadReportAsync();

    [RelayCommand]
    private async Task LoadReportAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Đang tải báo cáo...";

            var response = await _tcpClient.SendMessageAsync(
                MS2.Models.TCP.TcpActions.GET_SALES_REPORT,
                new { FromDate = FromDate, ToDate = ToDate.AddDays(1).AddSeconds(-1) },
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                var jsonString = response.Data?.ToString() ?? "{}";
                var report = System.Text.Json.JsonSerializer.Deserialize<MS2.Models.DTOs.Order.SalesReportDto>(
                    jsonString,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Orders.Clear();
                if (report != null)
                {
                    TotalRevenue = report.TotalRevenue;
                    TotalOrders = report.TotalOrders;
                    AverageOrderValue = report.AverageOrderValue;
                    if (report.Orders != null)
                        foreach (var order in report.Orders)
                            Orders.Add(order);
                }

                LastUpdated = $"Cập nhật: {DateTime.Now:HH:mm:ss}";
                StatusMessage = $"Tổng {Orders.Count} đơn | Khoảng {FromDate:dd/MM} – {ToDate:dd/MM}";
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
}
