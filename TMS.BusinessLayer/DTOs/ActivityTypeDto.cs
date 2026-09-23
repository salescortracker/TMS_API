using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class ActivityTypeDto
{
    public int ActivityTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActivityName { get; set; } = null!;

    public bool IsActive { get; set; }
}
