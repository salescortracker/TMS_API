using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Common;

/// <summary>Rules shared by My Timesheet, the clock card, approvals and bulk upload.</summary>
public interface ITimesheetCore
{
    Task<CandidateOnboarding> GetCandidateByUserAsync(int appUserId);

    Task<CandidateWorkSetting> GetSettingAsync(int candidateId);

    Task<int> GetIntSettingAsync(string key, int fallback);

    Task ValidateWorkDateAsync(DateOnly workDate, TimeZoneInfo zone);

    void ApplyTimes(TimesheetDay day, DateTime? inUtc, DateTime? outUtc, int breakMinutes);

    Task EvaluateFlagsAsync(TimesheetDay day, CandidateWorkSetting setting);

    Task EnsureWeekCapAsync(int candidateId, DateOnly weekStart, long excludeDayId, int thisDayMinutes, CandidateWorkSetting setting);

    string? ValidateComplete(TimesheetDay day);

    void ChangeStatus(TimesheetDay day, string to, int? byCandidateId, int? byAppUserId, string? comments);

    Task<int> EnsureApproverAsync(int appUserId);
}

public class TimesheetCore : ITimesheetCore
{
    private readonly TmsDbContext _db;

    public TimesheetCore(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<CandidateOnboarding> GetCandidateByUserAsync(int appUserId)
    {
        var candidate = await _db.CandidateOnboardings
            .Include(c => c.Team).Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.AppUserId == appUserId && c.IsActive);

        return candidate ?? throw new ForbiddenException("This account is not a candidate.");
    }

    public async Task<int> GetIntSettingAsync(string key, int fallback)
    {
        var value = await _db.SystemSettings
            .Where(s => s.SettingKey == key)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync();

        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    public async Task<CandidateWorkSetting> GetSettingAsync(int candidateId)
    {
        var setting = await _db.CandidateWorkSettings.FirstOrDefaultAsync(s => s.CandidateId == candidateId);
        if (setting is not null)
        {
            return setting;
        }

        setting = new CandidateWorkSetting
        {
            CandidateId = candidateId,
            WeeklyTargetHours = await GetIntSettingAsync(SettingKeys.WeeklyTargetHours, 40),
            BreakAlertMinutes = await GetIntSettingAsync(SettingKeys.BreakAlertMinutes, 60),
            WeekStartDay = 1,
            TimeZoneId = "UTC",
        };
        _db.CandidateWorkSettings.Add(setting);
        await _db.SaveChangesAsync();
        return setting;
    }

    public async Task ValidateWorkDateAsync(DateOnly workDate, TimeZoneInfo zone)
    {
        var today = TimeHelper.LocalToday(zone);
        if (workDate > today)
        {
            throw new BusinessRuleException("Future days can be submitted once the day has happened.");
        }

        var weeks = await GetIntSettingAsync(SettingKeys.MaxBackdateWeeks, 8);
        if (workDate < today.AddDays(-7 * weeks))
        {
            throw new BusinessRuleException($"That date is older than {weeks} weeks and can no longer be entered.");
        }
    }

    public void ApplyTimes(TimesheetDay day, DateTime? inUtc, DateTime? outUtc, int breakMinutes)
    {
        day.ClockInAt = inUtc;
        day.ClockOutAt = outUtc;
        day.BreakMinutes = breakMinutes;
        day.WorkedMinutes = inUtc is not null && outUtc is not null
            ? Math.Max(0, (int)(outUtc.Value - inUtc.Value).TotalMinutes - breakMinutes)
            : 0;
    }

    public async Task EvaluateFlagsAsync(TimesheetDay day, CandidateWorkSetting setting)
    {
        var reasons = new List<string>();
        var dailyCap = await GetIntSettingAsync(SettingKeys.DailyHoursCap, 8);

        if (day.WorkedMinutes > dailyCap * 60)
        {
            reasons.Add($"Exceeds the {dailyCap}-hour daily cap");
        }

        if (day.BreakMinutes > setting.BreakAlertMinutes)
        {
            reasons.Add($"Break over {setting.BreakAlertMinutes} minutes");
        }

        day.IsFlagged = reasons.Count > 0;
        day.FlagReason = reasons.Count > 0 ? string.Join("; ", reasons) : null;
    }

    public async Task EnsureWeekCapAsync(
        int candidateId, DateOnly weekStart, long excludeDayId, int thisDayMinutes, CandidateWorkSetting setting)
    {
        var others = await _db.TimesheetDays
            .Where(d => d.CandidateId == candidateId && d.WeekStartDate == weekStart && d.TimesheetDayId != excludeDayId)
            .SumAsync(d => (int?)d.WorkedMinutes) ?? 0;

        var total = others + thisDayMinutes;
        if (total > setting.WeeklyTargetHours * 60)
        {
            throw new BusinessRuleException(
                $"A week can never exceed {TimeHelper.FormatHours(setting.WeeklyTargetHours)} hours " +
                $"(this would make it {TimeHelper.FormatHours(Math.Round(total / 60m, 2))}).");
        }
    }

    public string? ValidateComplete(TimesheetDay day)
    {
        if (day.ClockInAt is null || day.ClockOutAt is null)
        {
            return "Log-in and log-out are required.";
        }

        if (day.ClockOutAt <= day.ClockInAt)
        {
            return "Log-out must be after log-in.";
        }

        if (string.IsNullOrWhiteSpace(day.Task) || day.Task.Trim().Length > 80)
        {
            return "Task is required (up to 80 characters).";
        }

        var descriptionLength = day.Description?.Trim().Length ?? 0;
        if (descriptionLength is < 10 or > 500)
        {
            return "Description needs 10 to 500 characters describing the work.";
        }

        return null;
    }

    public void ChangeStatus(TimesheetDay day, string to, int? byCandidateId, int? byAppUserId, string? comments)
    {
        day.TimesheetStatusHistories.Add(new TimesheetStatusHistory
        {
            FromStatus = string.IsNullOrEmpty(day.Status) ? null : day.Status,
            ToStatus = to,
            ChangedByCandidateId = byCandidateId,
            ChangedByAppUserId = byAppUserId,
            Comments = comments,
            ChangedAt = DateTime.UtcNow,
        });
        day.Status = to;
        day.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<int> EnsureApproverAsync(int appUserId)
    {
        var approver = await _db.Approvers.FirstOrDefaultAsync(a => a.AppUserId == appUserId);
        if (approver is not null)
        {
            return approver.ApproverId;
        }

        var user = await _db.AppUsers
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.AppUserId == appUserId);

        approver = new Approver
        {
            FullName = user.FullName,
            Email = user.Email,
            RoleLabel = user.UserRoles.Select(ur => ur.Role.RoleName).FirstOrDefault(),
            ApprovalOrder = 1,
            IsActive = true,
            AppUserId = appUserId,
        };
        _db.Approvers.Add(approver);
        await _db.SaveChangesAsync();
        return approver.ApproverId;
    }
}
