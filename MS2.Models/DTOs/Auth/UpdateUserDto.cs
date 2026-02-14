namespace MS2.Models.DTOs.Auth;

public class UpdateUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NewPassword { get; set; } // Optional - only if user wants to change password
}
