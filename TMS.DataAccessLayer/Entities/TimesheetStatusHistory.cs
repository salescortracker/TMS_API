using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetStatusHistory
{
    public long TimesheetStatusHistoryId { get; set; }

    public long TimesheetDayId { get; set; }

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = null!;

    public int? ChangedByCandidateId { get; set; }

    public int? ChangedByApproverId { get; set; }

    public string? Comments { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual Approver? ChangedByApprover { get; set; }

    public virtual CandidateOnboarding? ChangedByCandidate { get; set; }

    public virtual TimesheetDay TimesheetDay { get; set; } = null!;
}
