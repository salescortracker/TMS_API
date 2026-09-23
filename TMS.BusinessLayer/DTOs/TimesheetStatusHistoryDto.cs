using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TimesheetStatusHistoryDto
{
    public long TimesheetStatusHistoryId { get; set; }

    public long TimesheetDayId { get; set; }

    [MaxLength(20)]
    public string? FromStatus { get; set; }

    [Required]
    [MaxLength(20)]
    public string ToStatus { get; set; } = null!;

    public int? ChangedByCandidateId { get; set; }

    public int? ChangedByApproverId { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }

    public DateTime ChangedAt { get; set; }
}
