using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class CandidateOnboarding
{
    public int CandidateId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public int? TeamId { get; set; }

    public int? CompanyId { get; set; }

    public string? PhoneDialCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? AppUserId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<CandidateApproval> CandidateApprovals { get; set; } = new List<CandidateApproval>();

    public virtual CandidateWorkSetting? CandidateWorkSetting { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Team? Team { get; set; }

    public virtual ICollection<TimesheetAlert> TimesheetAlerts { get; set; } = new List<TimesheetAlert>();

    public virtual ICollection<TimesheetDay> TimesheetDays { get; set; } = new List<TimesheetDay>();

    public virtual ICollection<TimesheetStatusHistory> TimesheetStatusHistories { get; set; } = new List<TimesheetStatusHistory>();

    public virtual ICollection<TimesheetUploadBatch> TimesheetUploadBatches { get; set; } = new List<TimesheetUploadBatch>();
}
