using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class PermissionDto
{
    public int PermissionId { get; set; }

    [Required]
    [MaxLength(60)]
    public string PermissionCode { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }
}
