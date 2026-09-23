using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetBreak
{
    public long TimesheetBreakId { get; set; }

    public long TimesheetDayId { get; set; }

    public DateTime BreakStartAt { get; set; }

    public DateTime? BreakEndAt { get; set; }

    public int? DurationMinutes { get; set; }

    public bool AlertRaised { get; set; }

    public virtual ICollection<TimesheetAlert> TimesheetAlerts { get; set; } = new List<TimesheetAlert>();

    public virtual TimesheetDay TimesheetDay { get; set; } = null!;
}
