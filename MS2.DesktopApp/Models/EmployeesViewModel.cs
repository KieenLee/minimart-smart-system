using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.TCP;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private ObservableCollection<UserDto> employees = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "";

    public EmployeesViewModel(TcpClientService tcpClient, UserDto currentUser)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Initialize async - gọi sau khi constructor xong
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            await LoadEmployeesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi khởi tạo: {ex.Message}";
            MessageBox.Show($"Lỗi khi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Đang tải danh sách nhân viên...";

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.GET_EMPLOYEES,
                null,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                var jsonString = response.Data?.ToString() ?? "[]";
                var employeeList = JsonSerializer.Deserialize<List<UserDto>>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Employees.Clear();
                if (employeeList != null)
                {
                    foreach (var employee in employeeList)
                    {
                        Employees.Add(employee);
                    }
                }

                StatusMessage = $"Đã tải {Employees.Count} nhân viên";
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
