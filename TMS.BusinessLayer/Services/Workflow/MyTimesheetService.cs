using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IMyTimesheetService
{
    Task<WeekDto> GetWeekAsync(int appUserId, DateOnly? weekStart);

    Task<DayDto> SaveDayAsync(int appUserId, DateOnly date, SaveDayRequestDto request);

    Task<DayDto> SubmitDayAsync(int appUserId, DateOnly date);

    Task<SubmitResultDto> SubmitReadyAsync(int appUserId, DateOnly? weekStart);

    Task<List<HistoryWeekDto>> GetHistoryAsync(int appUserId);

    Task<CandidateDashboardDto> GetDashboardAsync(int appUserId);

    Task<ProfileDto> GetProfileAsync(int appUserId);

    Task<ProfileDto> UpdateProfileAsync(int appUserId, ProfileUpdateDto dto);
}

public class MyTimesheetService : IMyTimesheetService
{
    private readonly TmsDbContext _db;
    private readonly ITimesheetCore _core;
    private readonly IAuditService _audit;

    public MyTimesheetService(TmsDbContext db, ITimesheetCore core, IAuditService audit)
    {
        _db = db;
        _core = core;
        _audit = audit;
    }

    // ---------------- week ----------------

    public async Task<WeekDto> GetWeekAsync(int appUserId, DateOnly? weekStart)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var currentWeek = TimeHelper.WeekStart(today);
        var ws = weekStart is null ? currentWeek : TimeHelper.WeekStart(weekStart.Value);
        if (ws > currentWeek)
        {
            ws = currentWeek;
        }

        var rows = await _db.TimesheetDays
            .Where(d => d.CandidateId == c.CandidateId && d.WeekStartDate == ws)
            .ToListAsync();

        var week = new WeekDto
        {
            WeekStart = ws.ToString("yyyy-MM-dd"),
            WeekEnd = ws.AddDays(6).ToString("yyyy-MM-dd"),
            Label = $"Week of {ws:MMM d} – {ws.AddDays(6):MMM d, yyyy}",
            TargetHours = s.WeeklyTargetHours,
            LoggedHours = Math.Round(rows.Sum(r => r.WorkedMinutes) / 60m, 2),
            Approved = rows.Count(r => r.Status == DayStatus.Approved),
            Pending = rows.Count(r => r.Status == DayStatus.Pending),
            Rejected = rows.Count(r => r.Status == DayStatus.Rejected),
            Saved = rows.Count(r => r.Status == DayStatus.Saved),
            Drafts = rows.Count(r => r.Status == DayStatus.Draft),
            HasNext = ws < currentWeek,
            ReadyToSubmit = rows.Count(r =>
                r.Status is DayStatus.Draft or DayStatus.Saved or DayStatus.Rejected && _core.ValidateComplete(r) is null),
        };

        for (var i = 0; i < 7; i++)
        {
            var date = ws.AddDays(i);
            week.Days.Add(ToDayDto(date, rows.FirstOrDefault(r => r.WorkDate == date), zone, today));
        }

        return week;
    }

    private static DayDto ToDayDto(DateOnly date, TimesheetDay? row, TimeZoneInfo zone, DateOnly today)
    {
        var dto = new DayDto
        {
            Date = date.ToString("yyyy-MM-dd"),
            DayName = date.DayOfWeek.ToString(),
            DateLabel = date.ToString("MMM d", CultureInfo.InvariantCulture),
        };

        if (row is null)
        {
            dto.Status = date > today ? "Upcoming" : "Not entered";
            dto.Editable = date <= today;
            return dto;
        }

        dto.LogIn = TimeHelper.FormatHm(row.ClockInAt, zone);
        dto.LogOut = TimeHelper.FormatHm(row.ClockOutAt, zone);
        dto.BreakMinutes = row.BreakMinutes;
        dto.Task = row.Task ?? string.Empty;
        dto.Description = row.Description ?? string.Empty;
        dto.Status = row.Status;
        dto.Editable = row.Status is not (DayStatus.Pending or DayStatus.Approved);
        dto.Hours = row.ClockInAt is not null && row.ClockOutAt is not null ? Math.Round(row.WorkedMinutes / 60m, 2) : null;
        dto.RejectionReason = row.RejectionReason;
        dto.FlagReason = row.FlagReason;
        return dto;
    }

    // ---------------- save / submit ----------------

    public async Task<DayDto> SaveDayAsync(int appUserId, DateOnly date, SaveDayRequestDto req)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        await _core.ValidateWorkDateAsync(date, zone);

        var day = await _db.TimesheetDays.FirstOrDefaultAsync(d => d.CandidateId == c.CandidateId && d.WorkDate == date);
        if (day is not null && day.Status is DayStatus.Pending or DayStatus.Approved)
        {
            throw new BusinessRuleException($"This day is already {day!.Status.ToLowerInvariant()} and can't be edited.");
        }

        var saveAs = req.SaveAs == DayStatus.Saved ? DayStatus.Saved : DayStatus.Draft;
        var (inUtc, outUtc) = ParseTimes(date, req.LogIn, req.LogOut, zone);

        if (inUtc is not null && outUtc is not null &&
            req.BreakMinutes > (int)(outUtc.Value - inUtc.Value).TotalMinutes)
        {
            throw new BusinessRuleException("Break can't be longer than the time worked.");
        }

        var isNew = day is null;
        day ??= new TimesheetDay
        {
            CandidateId = c.CandidateId,
            WorkDate = date,
            WeekStartDate = TimeHelper.WeekStart(date),
            EntrySource = "Manual",
            Status = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _core.ApplyTimes(day, inUtc, outUtc, req.BreakMinutes);
        day.Task = string.IsNullOrWhiteSpace(req.Task) ? null : req.Task.Trim();
        day.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        day.RejectionReason = null;
        await _core.EvaluateFlagsAsync(day, s);

        if (inUtc is not null && outUtc is not null)
        {
            await _core.EnsureWeekCapAsync(c.CandidateId, day.WeekStartDate, day.TimesheetDayId, day.WorkedMinutes, s);
        }

        if (saveAs == DayStatus.Saved && _core.ValidateComplete(day) is { } error)
        {
            throw new BusinessRuleException(error);
        }

        if (isNew)
        {
            _db.TimesheetDays.Add(day);
        }

        if (day.Status != saveAs)
        {
            _core.ChangeStatus(day, saveAs, c.CandidateId, appUserId, null);
        }

        day.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDayDto(date, day, zone, today);
    }

    public async Task<DayDto> SubmitDayAsync(int appUserId, DateOnly date)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await _db.TimesheetDays.FirstOrDefaultAsync(d => d.CandidateId == c.CandidateId && d.WorkDate == date)
                  ?? throw new BusinessRuleException("Enter and save this day before submitting it.");

        await SubmitAsync(c, s, zone, day, appUserId);
        return ToDayDto(date, day, zone, today);
    }

    public async Task<SubmitResultDto> SubmitReadyAsync(int appUserId, DateOnly? weekStart)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var ws = TimeHelper.WeekStart(weekStart ?? today);
        var days = await _db.TimesheetDays
            .Where(d => d.CandidateId == c.CandidateId && d.WeekStartDate == ws &&
                        (d.Status == DayStatus.Draft || d.Status == DayStatus.Saved || d.Status == DayStatus.Rejected))
            .OrderBy(d => d.WorkDate)
            .ToListAsync();

        var result = new SubmitResultDto();
        foreach (var day in days)
        {
            try
            {
                await SubmitAsync(c, s, zone, day, appUserId);
                result.Submitted++;
            }
            catch (BusinessRuleException ex)
            {
                result.Skipped.Add($"{day.WorkDate:ddd MMM d}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task SubmitAsync(CandidateOnboarding c, CandidateWorkSetting s, TimeZoneInfo zone, TimesheetDay day, int appUserId)
    {
        if (day.Status is DayStatus.Pending or DayStatus.Approved)
        {
            throw new BusinessRuleException($"Already {day.Status.ToLowerInvariant()}.");
        }

        await _core.ValidateWorkDateAsync(day.WorkDate, zone);
        if (_core.ValidateComplete(day) is { } error)
        {
            throw new BusinessRuleException(error);
        }

        await _core.EnsureWeekCapAsync(c.CandidateId, day.WeekStartDate, day.TimesheetDayId, day.WorkedMinutes, s);

        _core.ChangeStatus(day, DayStatus.Pending, c.CandidateId, appUserId, null);
        day.SubmittedAt = DateTime.UtcNow;
        day.ReviewedAt = null;
        day.ReviewedByApproverId = null;
        day.ReviewedByAppUserId = null;
        day.RejectionReason = null;
        await _db.SaveChangesAsync();

        var name = $"{c.FirstName} {c.LastName}";
        await _audit.LogAsync(new AuditEntry(
            appUserId, LogCategory.Submissions, Severity.Success, "timesheet_submitted", "TimesheetDay",
            day.TimesheetDayId, c.CandidateId,
            $"{name} submitted {day.WorkDate:ddd MMM d}",
            null));
        await _audit.NotifyPermissionHoldersAsync(
            PermissionCodes.ApproveTimesheets, c.CandidateId, "Timesheet awaiting approval",
            $"{name} submitted {day.WorkDate:ddd MMM d} ({TimeHelper.FormatHours(Math.Round(day.WorkedMinutes / 60m, 2))} hrs).",
            "/super-admin/timesheet-approvals");
    }

    // ---------------- history / dashboard ----------------

    public async Task<List<HistoryWeekDto>> GetHistoryAsync(int appUserId)
    {
        var (c, _, zone, today) = await ContextAsync(appUserId);
        var currentWeek = TimeHelper.WeekStart(today);

        var rows = await _db.TimesheetDays
            .Where(d => d.CandidateId == c.CandidateId)
            .Select(d => new { d.WeekStartDate, d.Status, d.WorkedMinutes })
            .ToListAsync();

        var weeks = rows.GroupBy(r => r.WeekStartDate).ToDictionary(g => g.Key, g => g.ToList());
        weeks.TryAdd(currentWeek, new());

        return weeks.OrderByDescending(w => w.Key).Take(26).Select(w =>
        {
            var approved = w.Value.Count(r => r.Status == DayStatus.Approved);
            var pending = w.Value.Count(r => r.Status == DayStatus.Pending);
            var rejected = w.Value.Count(r => r.Status == DayStatus.Rejected);
            var drafts = w.Value.Count(r => r.Status is DayStatus.Draft or DayStatus.Saved);
            var badges = new List<BadgeDto>();
            if (approved > 0 || (pending + rejected + drafts) == 0)
            {
                badges.Add(new BadgeDto { Label = $"{approved} approved", Tone = "success" });
            }

            if (pending > 0) badges.Add(new BadgeDto { Label = $"{pending} pending", Tone = "warning" });
            if (rejected > 0) badges.Add(new BadgeDto { Label = $"{rejected} rejected", Tone = "danger" });
            if (drafts > 0) badges.Add(new BadgeDto { Label = $"{drafts} drafts", Tone = "neutral" });

            return new HistoryWeekDto
            {
                WeekStart = w.Key.ToString("yyyy-MM-dd"),
                WeekLabel = $"{w.Key:MMM d} – {w.Key.AddDays(6):MMM d, yyyy}",
                Hours = TimeHelper.FormatHours(Math.Round(w.Value.Sum(r => r.WorkedMinutes) / 60m, 2)) + " hrs",
                Badges = badges,
                IsCurrent = w.Key == currentWeek,
            };
        }).ToList();
    }

    public async Task<CandidateDashboardDto> GetDashboardAsync(int appUserId)
    {
        var (c, _, zone, today) = await ContextAsync(appUserId);
        var week = await GetWeekAsync(appUserId, null);
        var todayRow = week.Days.First(d => d.Date == today.ToString("yyyy-MM-dd"));

        return new CandidateDashboardDto
        {
            FullName = $"{c.FirstName} {c.LastName}",
            WeekLabel = $"{DateOnly.Parse(week.WeekStart):MMM d} – {DateOnly.Parse(week.WeekEnd):MMM d, yyyy}",
            Submitted = week.Approved + week.Pending + week.Rejected,
            Pending = week.Pending,
            Approved = week.Approved,
            Rejected = week.Rejected,
            Drafts = week.Drafts + week.Saved,
            WeekLoggedHours = week.LoggedHours,
            WeekTargetHours = week.TargetHours,
            TodayNotSubmitted = todayRow.Status is "Not entered" or "Draft" or "Saved" or "Rejected",
            Days = week.Days.Take(5).Select(d => new DashboardDayDto
            {
                Day = d.DayName[..3],
                Date = d.DateLabel,
                Status = d.Status,
                Hours = d.Hours is null ? null : $"{TimeHelper.FormatHours(d.Hours.Value)} hrs",
                Task = string.IsNullOrEmpty(d.Task) ? null : d.Task,
                IsToday = d.Date == today.ToString("yyyy-MM-dd"),
            }).ToList(),
        };
    }

    // ---------------- profile ----------------

    public async Task<ProfileDto> GetProfileAsync(int appUserId)
    {
        var c = await _core.GetCandidateByUserAsync(appUserId);
        return new ProfileDto
        {
            FirstName = c.FirstName, LastName = c.LastName, DateOfBirth = c.DateOfBirth, JoiningDate = c.JoiningDate,
            PhoneDialCode = c.PhoneDialCode, PhoneNumber = c.PhoneNumber, Email = c.Email ?? string.Empty,
            TeamId = c.TeamId, CompanyId = c.CompanyId,
        };
    }

    public async Task<ProfileDto> UpdateProfileAsync(int appUserId, ProfileUpdateDto dto)
    {
        var c = await _core.GetCandidateByUserAsync(appUserId);
        var email = dto.Email.Trim();

        if (await _db.AppUsers.AnyAsync(u => u.Email == email && u.AppUserId != appUserId) ||
            await _db.CandidateOnboardings.AnyAsync(x => x.Email == email && x.CandidateId != c.CandidateId))
        {
            throw new BusinessRuleException("That email is already used by another account.");
        }

        c.FirstName = dto.FirstName.Trim();
        c.LastName = dto.LastName.Trim();
        c.DateOfBirth = dto.DateOfBirth;
        c.JoiningDate = dto.JoiningDate;
        c.PhoneDialCode = dto.PhoneDialCode;
        c.PhoneNumber = dto.PhoneNumber.Trim();
        c.Email = email;
        c.TeamId = dto.TeamId ?? c.TeamId;
        c.CompanyId = dto.CompanyId ?? c.CompanyId;
        c.UpdatedAt = DateTime.UtcNow;

        var user = await _db.AppUsers.FirstAsync(u => u.AppUserId == appUserId);
        user.FullName = $"{c.FirstName} {c.LastName}";
        user.Email = email;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(new AuditEntry(
            appUserId, LogCategory.EditsRoles, Severity.Info, "profile_updated", "CandidateOnboarding",
            c.CandidateId, c.CandidateId, $"{user.FullName} updated their profile", null));

        return await GetProfileAsync(appUserId);
    }

    // ---------------- helpers ----------------

    private async Task<(CandidateOnboarding C, CandidateWorkSetting S, TimeZoneInfo Zone, DateOnly Today)> ContextAsync(int appUserId)
    {
        var c = await _core.GetCandidateByUserAsync(appUserId);
        var s = await _core.GetSettingAsync(c.CandidateId);
        var zone = TimeHelper.GetZone(s.TimeZoneId);
        return (c, s, zone, TimeHelper.LocalToday(zone));
    }

    private static (DateTime? In, DateTime? Out) ParseTimes(DateOnly date, string? logIn, string? logOut, TimeZoneInfo zone)
    {
        DateTime? inUtc = null;
        DateTime? outUtc = null;

        if (!string.IsNullOrWhiteSpace(logIn))
        {
            if (!TimeHelper.TryParseTime(logIn, out var t))
            {
                throw new BusinessRuleException("Log-in must be a valid time like 09:00.");
            }

            inUtc = TimeHelper.ToUtc(date, t, zone);
        }

        if (!string.IsNullOrWhiteSpace(logOut))
        {
            if (!TimeHelper.TryParseTime(logOut, out var t))
            {
                throw new BusinessRuleException("Log-out must be a valid time like 17:30.");
            }

            outUtc = TimeHelper.ToUtc(date, t, zone);
        }

        if (inUtc is not null && outUtc is not null && outUtc <= inUtc)
        {
            throw new BusinessRuleException("Log-out must be after log-in.");
        }

        return (inUtc, outUtc);
    }
}
