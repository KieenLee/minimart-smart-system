using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.TCP;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

    [ObservableProperty]
    private string searchKeyword = "";

    // Properties for Create User Dialog
    [ObservableProperty]
    private string newUsername = "";

    [ObservableProperty]
    private string newPassword = "";

    [ObservableProperty]
    private string newFullName = "";

    [ObservableProperty]
    private string newEmail = "";

    [ObservableProperty]
    private string newPhone = "";

    [ObservableProperty]
    private string newAddress = "";

    [ObservableProperty]
    private string newRole = "Employee";

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

    [RelayCommand]
    private async Task SearchEmployeesAsync()
    {
        try
        {
            IsLoading = true;

            // Nếu search keyword rỗng, load tất cả
            if (string.IsNullOrWhiteSpace(SearchKeyword))
            {
                await LoadEmployeesAsync();
                return;
            }

            StatusMessage = $"Đang tìm kiếm '{SearchKeyword}'...";

            var searchData = new { Keyword = SearchKeyword };
            var response = await _tcpClient.SendMessageAsync(
                TcpActions.SEARCH_USERS,
                searchData,
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

                StatusMessage = $"Tìm thấy {Employees.Count} nhân viên";
            }
            else
            {
                StatusMessage = $"Lỗi: {response?.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
            MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateEmployeeAsync()
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(NewUsername))
            {
                MessageBox.Show("Vui lòng nhập Username", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBox.Show("Vui lòng nhập Password", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewFullName))
            {
                MessageBox.Show("Vui lòng nhập Họ tên", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewEmail))
            {
                MessageBox.Show("Vui lòng nhập Email", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            StatusMessage = "Đang tạo nhân viên mới...";

            var createUserDto = new CreateUserDto
            {
                Username = NewUsername.Trim(),
                Password = NewPassword,
                FullName = NewFullName.Trim(),
                Email = NewEmail.Trim(),
                Phone = NewPhone?.Trim(),
                Address = NewAddress?.Trim(),
                Role = NewRole
            };

            var response = await _tcpClient.SendMessageAsync(
                TcpActions.CREATE_USER,
                createUserDto,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                MessageBox.Show("Tạo nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                ClearCreateEmployeeForm();

                // Reload list
                await LoadEmployeesAsync();
            }
            else
            {
                StatusMessage = $"Lỗi: {response?.Message}";
                MessageBox.Show($"Lỗi: {response?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Lỗi: {ex.Message}";
            MessageBox.Show($"Lỗi tạo nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearCreateEmployeeForm()
    {
        NewUsername = "";
        NewPassword = "";
        NewFullName = "";
        NewEmail = "";
        NewPhone = "";
        NewAddress = "";
        NewRole = "Employee";
    }
}
