using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs.Workflow;

// ---------- Timesheet approvals ----------

public class ApprovalEntryDto
{
    public string Id { get; set; } = null!;
    public int CandidateId { get; set; }
    public string Candidate { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Day { get; set; } = null!;            // Wed, Sep 23
    public string WorkDate { get; set; } = null!;       // 2026-09-23
    public string TimeRange { get; set; } = null!;      // 09:00 AM – 04:30 PM
    public string LogIn { get; set; } = string.Empty;   // 09:00
    public string LogOut { get; set; } = string.Empty;
    public int BreakMinutes { get; set; }
    public string Task { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Hours { get; set; } = null!;          // 7 hrs
    public string WeekProgress { get; set; } = null!;   // Week 22.5 / 40
    public string Checks { get; set; } = null!;         // Clean | Flagged
    public string? FlagReason { get; set; }
    public string Decision { get; set; } = null!;       // Pending | Approved | Rejected
    public string? RejectionReason { get; set; }
}

public class ApprovalListDto
{
    public List<ApprovalEntryDto> Items { get; set; } = new();
}

public class ApprovalRejectDto
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = null!;
}

public class ApprovalEditDto
{
    [Required]
    public string LogIn { get; set; } = null!;

    [Required]
    public string LogOut { get; set; } = null!;

    [Range(0, 600)]
    public int BreakMinutes { get; set; }

    [MaxLength(80)]
    public string? Task { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class ApproveManyDto
{
    public List<long> Ids { get; set; } = new();
}

public class ApproveManyResultDto
{
    public int Approved { get; set; }
    public List<string> Skipped { get; set; } = new();
}

// ---------- Admin dashboard ----------

public class StatCardDto
{
    public string Label { get; set; } = null!;
    public int Value { get; set; }
    public string? Tone { get; set; }
    public bool Highlight { get; set; }
}

public class CompanyTeamRowDto
{
    public string Name { get; set; } = null!;
    public int Candidates { get; set; }
    public int ClockedIn { get; set; }
    public int Submitted { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int NotSubmitted { get; set; }
}

public class AttendanceRowDto
{
    public string Name { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string TimeRange { get; set; } = null!;
    public string BreakLabel { get; set; } = null!;
    public bool BreakAlert { get; set; }
    public string Status { get; set; } = null!;
    public string StatusTone { get; set; } = null!;
}

public class DaySegmentDto
{
    public string Day { get; set; } = null!;
    public int? Total { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public int NotSubmitted { get; set; }
    public string Note { get; set; } = null!;
}

public class WaitingItemDto
{
    public string Name { get; set; } = null!;
    public string Since { get; set; } = null!;
    public int Days { get; set; }
}

public class ActivityItemDto
{
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = null!;
    public string? Description { get; set; }
    public string Category { get; set; } = null!;
    public string Time { get; set; } = null!;   // ISO UTC
    public string Tone { get; set; } = null!;   // success | warning | danger | neutral
}

public class DashboardDto
{
    public string WeekLabel { get; set; } = null!;
    public string ScopeLabel { get; set; } = null!;
    public List<StatCardDto> Stats { get; set; } = new();
    public List<CompanyTeamRowDto> TeamRows { get; set; } = new();
    public List<AttendanceRowDto> Attendance { get; set; } = new();
    public List<DaySegmentDto> WeekSegments { get; set; } = new();
    public List<WaitingItemDto> Waiting { get; set; } = new();
    public List<ActivityItemDto> Recent { get; set; } = new();
}

// ---------- Timesheets report ----------

public class TeamSummaryRowDto
{
    public string Name { get; set; } = null!;
    public int Candidates { get; set; }
    public string Hours { get; set; } = null!;
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
}

public class CandidateSummaryRowDto
{
    public int CandidateId { get; set; }
    public string Name { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string WeekHours { get; set; } = null!;
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
}

public class PeriodOptionDto
{
    public string Value { get; set; } = null!;   // 2026-09-21 (week start) or 2026-09 (month)
    public string Label { get; set; } = null!;
}

public class TimesheetReportDto
{
    public string Mode { get; set; } = "weekly";
    public string PeriodLabel { get; set; } = null!;
    public string PeriodValue { get; set; } = null!;
    public List<PeriodOptionDto> Periods { get; set; } = new();
    public int TotalSubmitted { get; set; }
    public int TotalApproved { get; set; }
    public int TotalPending { get; set; }
    public int TotalRejected { get; set; }
    public List<TeamSummaryRowDto> TeamRows { get; set; } = new();
    public List<CandidateSummaryRowDto> Candidates { get; set; } = new();
}

// ---------- People ----------

public class PersonListItemDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Since { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class MonthSubmissionDto
{
    public string Label { get; set; } = null!;
    public int DaysSubmitted { get; set; }
    public string Hours { get; set; } = null!;
    public int Approved { get; set; }
    public int Pending { get; set; }
    public string LastSubmission { get; set; } = null!;
    public string Tone { get; set; } = null!;
}

public class PersonDetailDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = "—";
    public string Team { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Since { get; set; } = null!;
    public string DateOfBirth { get; set; } = "—";
    public string OnboardedOn { get; set; } = "—";
    public string ApprovedBy { get; set; } = "—";
    public string AccountStatus { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string TimesheetsSummary { get; set; } = null!;
    public List<MonthSubmissionDto> Timeline { get; set; } = new();
}

public class StaffMemberDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Phone { get; set; } = "—";
    public string Company { get; set; } = null!;
    public string Team { get; set; } = null!;
    public string Since { get; set; } = null!;
    public string AccountStatus { get; set; } = null!;
}

// ---------- Activity log ----------

public class ActivityEntryDto
{
    public long Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = null!;
    public string Tone { get; set; } = null!;
    public string Time { get; set; } = null!;
}

public class EmailAlertDto
{
    public long Id { get; set; }
    public string Recipient { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Time { get; set; } = null!;
}

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Message { get; set; }
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public string Time { get; set; } = null!;
}

public class NotificationsDto
{
    public int Unread { get; set; }
    public List<NotificationDto> Items { get; set; } = new();
}
