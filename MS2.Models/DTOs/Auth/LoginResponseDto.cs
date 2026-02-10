namespace MS2.Models.DTOs.Auth;

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = null!;
    public string Message { get; set; } = null!;
    public UserDto? User { get; set; }
}