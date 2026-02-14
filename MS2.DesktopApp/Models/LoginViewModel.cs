using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.TCP;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MS2.DesktopApp.Models;

public partial class LoginViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;

    [ObservableProperty]
    private string username = "";

    [ObservableProperty]
    private string password = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private bool isLoading = false;

    public LoginViewModel(TcpClientService tcpClient)
    {
        _tcpClient = tcpClient;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        // Validate
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Vui lòng nhập tên đăng nhập!";
            ErrorVisibility = Visibility.Visible;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập mật khẩu!";
            ErrorVisibility = Visibility.Visible;
            return;
        }

        IsLoading = true;
        ErrorMessage = "";
        ErrorVisibility = Visibility.Collapsed;

        try
        {
            // 1. Kết nối TCP Server
            bool connected = await _tcpClient.ConnectAsync();
            if (!connected)
            {
                ErrorMessage = "Không thể kết nối tới server!\nVui lòng kiểm tra server đã chạy chưa.";
                ErrorVisibility = Visibility.Visible;
                IsLoading = false;
                return;
            }

            // 2. Tạo LoginRequest
            var loginRequest = new LoginRequestDto
            {
                Username = Username,
                Password = Password
            };

            // 3. Gửi LOGIN request
            var response = await _tcpClient.SendMessageAsync(
                TcpActions.LOGIN,
                loginRequest,
                sessionId: null  // LOGIN không cần SessionId
            );

            // 4. Xử lý response
            if (response?.Success == true)
            {
                // Parse LoginResponse
                var jsonString = response.Data?.ToString() ?? "{}";
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (loginResponse?.SessionId != null)
                {
                    // Lưu SessionId vào TcpClient
                    _tcpClient.CurrentSessionId = loginResponse.SessionId;

                    // Đóng LoginWindow và mở MainWindow
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Get ServiceProvider from App
                        var app = (App)Application.Current;
                        var serviceProvider = app.ServiceProvider;

                        if (serviceProvider != null && loginResponse.User != null)
                        {
                            // Get MainWindow và MainViewModel từ DI
                            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
                            var mainViewModel = ActivatorUtilities.CreateInstance<MainViewModel>(
                                serviceProvider,
                                loginResponse.User
                            );
                            mainWindow.DataContext = mainViewModel;
                            mainWindow.Show();

                            // Tìm và đóng LoginWindow
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window is Presentation.LoginWindow)
                                {
                                    window.Close();
                                    break;
                                }
                            }
                        }
                    });
                }
                else
                {
                    ErrorMessage = "Lỗi: Không nhận được SessionId từ server!";
                    ErrorVisibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessage = response?.Message ?? "Đăng nhập thất bại! Vui lòng kiểm tra lại thông tin.";
                ErrorVisibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi kết nối: {ex.Message}";
            ErrorVisibility = Visibility.Visible;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
