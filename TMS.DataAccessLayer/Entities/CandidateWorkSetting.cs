using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class CandidateWorkSetting
{
    public int CandidateId { get; set; }

    public decimal WeeklyTargetHours { get; set; }

    public int BreakAlertMinutes { get; set; }

    public byte WeekStartDay { get; set; }

    public string TimeZoneId { get; set; } = null!;

    public virtual CandidateOnboarding Candidate { get; set; } = null!;
}
