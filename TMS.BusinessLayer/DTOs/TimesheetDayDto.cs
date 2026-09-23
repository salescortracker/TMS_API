using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TimesheetDayDto
{
    public long TimesheetDayId { get; set; }

    public int CandidateId { get; set; }

    public DateOnly WorkDate { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateTime? ClockInAt { get; set; }

    public DateTime? ClockOutAt { get; set; }

    public int? ActivityTypeId { get; set; }

    public int WorkedMinutes { get; set; }

    public int BreakMinutes { get; set; }

    public int BreakCount { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [Required]
    [MaxLength(10)]
    public string EntrySource { get; set; } = null!;

    public int? UploadBatchId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByApproverId { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public decimal? WorkedHours { get; set; }
}
