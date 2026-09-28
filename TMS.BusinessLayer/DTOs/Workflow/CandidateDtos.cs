using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs.Workflow;

public class DayDto
{
    public string Date { get; set; } = null!;          // 2026-09-21
    public string DayName { get; set; } = null!;       // Monday
    public string DateLabel { get; set; } = null!;     // Sep 21
    public string LogIn { get; set; } = string.Empty;  // 09:00 (candidate local time)
    public string LogOut { get; set; } = string.Empty;
    public int BreakMinutes { get; set; }
    public string Task { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = null!;        // Draft|Saved|Pending|Approved|Rejected|Not entered|Upcoming
    public bool Editable { get; set; }
    public decimal? Hours { get; set; }
    public string? RejectionReason { get; set; }
    public string? FlagReason { get; set; }
}

public class WeekDto
{
    public string WeekStart { get; set; } = null!;
    public string WeekEnd { get; set; } = null!;
    public string Label { get; set; } = null!;
    public decimal TargetHours { get; set; }
    public decimal LoggedHours { get; set; }
    public int Approved { get; set; }
    public int Pending { get; set; }
    public int Rejected { get; set; }
    public int Saved { get; set; }
    public int Drafts { get; set; }
    public int ReadyToSubmit { get; set; }
    public bool HasNext { get; set; }
    public List<DayDto> Days { get; set; } = new();
}

public class SaveDayRequestDto
{
    public string? LogIn { get; set; }
    public string? LogOut { get; set; }

    [Range(0, 600)]
    public int BreakMinutes { get; set; }

    [MaxLength(80)]
    public string? Task { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>Draft or Saved.</summary>
    public string SaveAs { get; set; } = "Draft";
}

public class SubmitResultDto
{
    public int Submitted { get; set; }
    public List<string> Skipped { get; set; } = new();
}

public class BadgeDto
{
    public string Label { get; set; } = null!;
    public string Tone { get; set; } = null!;
}

public class HistoryWeekDto
{
    public string WeekStart { get; set; } = null!;
    public string WeekLabel { get; set; } = null!;
    public string Hours { get; set; } = null!;
    public List<BadgeDto> Badges { get; set; } = new();
    public bool IsCurrent { get; set; }
}

public class DashboardDayDto
{
    public string Day { get; set; } = null!;
    public string Date { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Hours { get; set; }
    public string? Task { get; set; }
    public bool IsToday { get; set; }
}

public class CandidateDashboardDto
{
    public string FullName { get; set; } = null!;
    public string WeekLabel { get; set; } = null!;
    public int Submitted { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Drafts { get; set; }
    public decimal WeekLoggedHours { get; set; }
    public decimal WeekTargetHours { get; set; }
    public bool TodayNotSubmitted { get; set; }
    public List<DashboardDayDto> Days { get; set; } = new();
}

public class ClockStateDto
{
    /// <summary>NotClockedIn | Working | OnBreak | ClockedOut</summary>
    public string Status { get; set; } = "NotClockedIn";
    public DateTime? ClockInAt { get; set; }
    public DateTime? ClockOutAt { get; set; }
    public DateTime? OnBreakSince { get; set; }
    public int BreakMinutes { get; set; }
    public int BreakCount { get; set; }
    public int WorkedMinutes { get; set; }
    public string LocalDate { get; set; } = null!;
    public string TimeZoneId { get; set; } = null!;
    public string? Message { get; set; }
}

public class ProfileDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? PhoneDialCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = null!;
    public int? TeamId { get; set; }
    public int? CompanyId { get; set; }
}

public class ProfileUpdateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? JoiningDate { get; set; }

    [MaxLength(6)]
    public string? PhoneDialCode { get; set; }

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = null!;

    public int? TeamId { get; set; }

    public int? CompanyId { get; set; }
}
