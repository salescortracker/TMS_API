using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetDay
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

    public string? Notes { get; set; }

    public string EntrySource { get; set; } = null!;

    public int? UploadBatchId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByApproverId { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public decimal? WorkedHours { get; set; }

    public virtual ActivityType? ActivityType { get; set; }

    public virtual CandidateOnboarding Candidate { get; set; } = null!;

    public virtual Approver? ReviewedByApprover { get; set; }

    public virtual ICollection<TimesheetAlert> TimesheetAlerts { get; set; } = new List<TimesheetAlert>();

    public virtual ICollection<TimesheetBreak> TimesheetBreaks { get; set; } = new List<TimesheetBreak>();

    public virtual ICollection<TimesheetStatusHistory> TimesheetStatusHistories { get; set; } = new List<TimesheetStatusHistory>();

    public virtual TimesheetUploadBatch? UploadBatch { get; set; }
}
