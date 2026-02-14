using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MS2.DesktopApp.Network;
using MS2.Models.DTOs.Auth;
using MS2.Models.TCP;
using System.Text.Json;
using System.Threading.Tasks;

namespace MS2.DesktopApp.Models;

public partial class ProfileViewModel : ObservableObject
{
    private readonly TcpClientService _tcpClient;
    private readonly UserDto _currentUser;

    [ObservableProperty]
    private int userId;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string? phone;

    [ObservableProperty]
    private string? address;

    [ObservableProperty]
    private string role = string.Empty;

    [ObservableProperty]
    private string? newPassword;

    [ObservableProperty]
    private string? confirmPassword;

    [ObservableProperty]
    private bool isLoading = false;

    public ProfileViewModel(TcpClientService tcpClient, UserDto currentUser)
    {
        _tcpClient = tcpClient;
        _currentUser = currentUser;

        // Load current user data
        LoadUserData();
    }

    private void LoadUserData()
    {
        UserId = _currentUser.Id;
        Username = _currentUser.Username;
        FullName = _currentUser.FullName;
        Email = _currentUser.Email;
        Phone = _currentUser.Phone;
        Address = _currentUser.Address;
        Role = _currentUser.Role;
    }

    [RelayCommand]
    private async Task UpdateProfileAsync()
    {
        try
        {
            IsLoading = true;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(FullName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                return;
            }

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword != ConfirmPassword)
                {
                    return;
                }

                if (NewPassword.Length < 6)
                {
                    return;
                }
            }

            // Create update DTO
            var updateUserDto = new UpdateUserDto
            {
                UserId = UserId,
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Address = Address,
                NewPassword = string.IsNullOrWhiteSpace(NewPassword) ? null : NewPassword
            };

            // Send update request
            var response = await _tcpClient.SendMessageAsync(
                TcpActions.UPDATE_USER_PROFILE,
                updateUserDto,
                _tcpClient.CurrentSessionId
            );

            if (response?.Success == true)
            {
                // Parse updated user
                var jsonString = response.Data?.ToString() ?? "{}";
                var updatedUser = JsonSerializer.Deserialize<UserDto>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (updatedUser != null)
                {
                    // Update current user in session
                    _currentUser.FullName = updatedUser.FullName;
                    _currentUser.Email = updatedUser.Email;
                    _currentUser.Phone = updatedUser.Phone;
                    _currentUser.Address = updatedUser.Address;
                }

                // Clear password fields
                NewPassword = null;
                ConfirmPassword = null;
            }
        }
        catch
        {
            // Silent error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ResetForm()
    {
        LoadUserData();
        NewPassword = null;
        ConfirmPassword = null;
    }
}
