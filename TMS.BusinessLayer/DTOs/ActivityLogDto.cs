using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class ActivityLogDto
{
    public long ActivityLogId { get; set; }

    public int? ActorAppUserId { get; set; }

    [Required]
    [MaxLength(40)]
    public string ActionType { get; set; } = null!;

    [Required]
    [MaxLength(40)]
    public string EntityType { get; set; } = null!;

    public long? EntityId { get; set; }

    public int? TargetCandidateId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Severity { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = null!;

    public string? DetailsJson { get; set; }

    public DateTime CreatedAt { get; set; }
}
