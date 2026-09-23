using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class RoleDto
{
    public int RoleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RoleName { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }
}
