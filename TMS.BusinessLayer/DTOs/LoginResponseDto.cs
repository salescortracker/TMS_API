namespace TMS.BusinessLayer.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public int AppUserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool MustChangePassword { get; set; }

    public List<string> Roles { get; set; } = new();

    public List<string> Permissions { get; set; } = new();

    public List<string> Menus { get; set; } = new();
}

public class ChangePasswordDto
{
    [System.ComponentModel.DataAnnotations.Required]
    public string CurrentPassword { get; set; } = null!;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(8)]
    public string NewPassword { get; set; } = null!;
}
