using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using ClosedXML.Excel;
using Microsoft.Win32;

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

        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _autoRefreshTimer.Tick += async (s, e) => await LoadReportAsync();
        _autoRefreshTimer.Start();
    }

    public void Cleanup() => _autoRefreshTimer?.Stop();

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

    [RelayCommand]
    private void ExportReport()
    {
        if (Orders.Count == 0)
        {
            MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Lưu báo cáo doanh thu",
            Filter = "Excel Workbook (*.xlsx)|*.xlsx",
            FileName = $"BaoCaoDoanhThu_{FromDate:ddMMyyyy}_{ToDate:ddMMyyyy}.xlsx",
            DefaultExt = "xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Doanh Thu");

            // Tiêu đề
            ws.Range("A1:F1").Merge();
            ws.Cell("A1").Value = "BÁO CÁO DOANH THU";
            ws.Cell("A1").Style.Font.Bold = true;
            ws.Cell("A1").Style.Font.FontSize = 16;
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#1A2340");
            ws.Cell("A1").Style.Font.FontColor = XLColor.White;

            ws.Range("A2:F2").Merge();
            ws.Cell("A2").Value = $"Từ ngày {FromDate:dd/MM/yyyy} đến {ToDate:dd/MM/yyyy}";
            ws.Cell("A2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A2").Style.Font.Italic = true;

            // KPI
            ws.Cell("A4").Value = "Tổng Doanh Thu (đ)";
            ws.Cell("B4").Value = (double)TotalRevenue;
            ws.Cell("B4").Style.NumberFormat.Format = "#,##0";
            ws.Cell("B4").Style.Font.Bold = true;
            ws.Cell("D4").Value = "Số Đơn Hàng";
            ws.Cell("E4").Value = TotalOrders;
            ws.Cell("A5").Value = "Giá Trị TB/Đơn (đ)";
            ws.Cell("B5").Value = (double)AverageOrderValue;
            ws.Cell("B5").Style.NumberFormat.Format = "#,##0";

            // Header
            int headerRow = 7;
            var headers = new[] { "STT", "Mã Đơn", "Ngày Đặt", "Tổng Tiền (đ)", "Trạng Thái", "Ghi Chú" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2563EB");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Data rows
            int row = headerRow + 1;
            int stt = 1;
            foreach (var order in Orders)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = order.Id;
                ws.Cell(row, 3).Value = order.OrderDate.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 4).Value = (double)order.TotalAmount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
                ws.Cell(row, 5).Value = order.Status ?? "";
                ws.Cell(row, 6).Value = order.Notes ?? "";
                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                row++;
            }

            // Tổng cộng
            ws.Cell(row, 3).Value = "TỔNG CỘNG";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).FormulaA1 = $"=SUM(D{headerRow + 1}:D{row - 1})";
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#DC2626");

            ws.Columns().AdjustToContents();
            wb.SaveAs(dialog.FileName);

            MessageBox.Show($"✅ Xuất báo cáo thành công!\n{dialog.FileName}", "Xuất Excel",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi xuất Excel: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
