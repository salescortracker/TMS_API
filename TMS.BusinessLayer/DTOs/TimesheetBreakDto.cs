using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TimesheetBreakDto
{
    public long TimesheetBreakId { get; set; }

    public long TimesheetDayId { get; set; }

    public DateTime BreakStartAt { get; set; }

    public DateTime? BreakEndAt { get; set; }

    public int? DurationMinutes { get; set; }

    public bool AlertRaised { get; set; }
}
