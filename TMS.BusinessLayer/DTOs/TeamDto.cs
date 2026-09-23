using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TeamDto
{
    public int TeamId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TeamName { get; set; } = null!;

    [Required]
    [MaxLength(6)]
    public string DialCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
