using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TimesheetAlertDto
{
    public long TimesheetAlertId { get; set; }

    public int CandidateId { get; set; }

    public long? TimesheetDayId { get; set; }

    public long? TimesheetBreakId { get; set; }

    [Required]
    [MaxLength(30)]
    public string AlertType { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = null!;

    public bool NotifyCandidate { get; set; }

    public bool NotifyAdmin { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
