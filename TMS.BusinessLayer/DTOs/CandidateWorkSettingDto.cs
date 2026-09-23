using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class CandidateWorkSettingDto
{
    public int CandidateId { get; set; }

    public decimal WeeklyTargetHours { get; set; }

    public int BreakAlertMinutes { get; set; }

    public byte WeekStartDay { get; set; }

    [Required]
    [MaxLength(64)]
    public string TimeZoneId { get; set; } = null!;
}
