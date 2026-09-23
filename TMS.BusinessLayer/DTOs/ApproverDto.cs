using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class ApproverDto
{
    public int ApproverId { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [MaxLength(254)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? RoleLabel { get; set; }

    public byte ApprovalOrder { get; set; }

    public bool IsActive { get; set; }

    public int? AppUserId { get; set; }
}
