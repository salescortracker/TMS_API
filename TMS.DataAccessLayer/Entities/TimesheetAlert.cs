using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetAlert
{
    public long TimesheetAlertId { get; set; }

    public int CandidateId { get; set; }

    public long? TimesheetDayId { get; set; }

    public long? TimesheetBreakId { get; set; }

    public string AlertType { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool NotifyCandidate { get; set; }

    public bool NotifyAdmin { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CandidateOnboarding Candidate { get; set; } = null!;

    public virtual TimesheetBreak? TimesheetBreak { get; set; }

    public virtual TimesheetDay? TimesheetDay { get; set; }
}
