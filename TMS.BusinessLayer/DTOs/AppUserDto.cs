using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class AppUserDto
{
    public int AppUserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required]
    [MaxLength(254)]
    public string Email { get; set; } = null!;

    public byte[]? PasswordHash { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
