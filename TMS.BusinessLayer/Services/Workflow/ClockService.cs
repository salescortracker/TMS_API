using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

/// <summary>The clock-in card: clock in/out and breaks, using the server's clock.</summary>
public interface IClockService
{
    Task<ClockStateDto> GetStateAsync(int appUserId);

    Task<ClockStateDto> ClockInAsync(int appUserId);

    Task<ClockStateDto> ClockOutAsync(int appUserId);

    Task<ClockStateDto> BreakStartAsync(int appUserId);

    Task<ClockStateDto> BreakEndAsync(int appUserId);
}

public class ClockService : IClockService
{
    private readonly TmsDbContext _db;
    private readonly ITimesheetCore _core;
    private readonly IAuditService _audit;

    public ClockService(TmsDbContext db, ITimesheetCore core, IAuditService audit)
    {
        _db = db;
        _core = core;
        _audit = audit;
    }

    public async Task<ClockStateDto> GetStateAsync(int appUserId)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await FindTodayAsync(c.CandidateId, today);
        return await BuildStateAsync(day, zone, today, null);
    }

    public async Task<ClockStateDto> ClockInAsync(int appUserId)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await FindTodayAsync(c.CandidateId, today);

        if (day is not null && day.Status is DayStatus.Pending or DayStatus.Approved)
        {
            throw new BusinessRuleException($"Today is already {day!.Status.ToLowerInvariant()}.");
        }

        if (day?.ClockInAt is not null)
        {
            throw new BusinessRuleException(day.ClockOutAt is null
                ? "You're already clocked in."
                : "You've already clocked out today. Edit the times in My Timesheet if they're wrong.");
        }

        if (day is null)
        {
            day = new TimesheetDay
            {
                CandidateId = c.CandidateId, WorkDate = today, WeekStartDate = TimeHelper.WeekStart(today),
                EntrySource = "Clock", Status = string.Empty, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            };
            _db.TimesheetDays.Add(day);
            _core.ChangeStatus(day, DayStatus.Draft, c.CandidateId, appUserId, "Clocked in");
        }

        day.ClockInAt = DateTime.UtcNow;
        day.ClockOutAt = null;
        await _db.SaveChangesAsync();
        return await BuildStateAsync(day, zone, today, "Clocked in.");
    }

    public async Task<ClockStateDto> ClockOutAsync(int appUserId)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await RequireClockedInAsync(c.CandidateId, today);

        var open = await OpenBreakAsync(day.TimesheetDayId);
        if (open is not null)
        {
            await EndBreakAsync(c, s, day, open, appUserId);
        }

        _core.ApplyTimes(day, day.ClockInAt, DateTime.UtcNow, day.BreakMinutes);
        await _core.EvaluateFlagsAsync(day, s);
        day.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await BuildStateAsync(day, zone, today, "Clocked out. Add a task and description in My Timesheet, then submit.");
    }

    public async Task<ClockStateDto> BreakStartAsync(int appUserId)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await RequireClockedInAsync(c.CandidateId, today);

        if (await OpenBreakAsync(day.TimesheetDayId) is not null)
        {
            throw new BusinessRuleException("You're already on a break.");
        }

        _db.TimesheetBreaks.Add(new TimesheetBreak { TimesheetDayId = day.TimesheetDayId, BreakStartAt = DateTime.UtcNow });
        day.BreakCount++;
        day.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await BuildStateAsync(day, zone, today, "Break started.");
    }

    public async Task<ClockStateDto> BreakEndAsync(int appUserId)
    {
        var (c, s, zone, today) = await ContextAsync(appUserId);
        var day = await RequireClockedInAsync(c.CandidateId, today);
        var open = await OpenBreakAsync(day.TimesheetDayId)
                   ?? throw new BusinessRuleException("You're not on a break.");

        await EndBreakAsync(c, s, day, open, appUserId);
        return await BuildStateAsync(day, zone, today, "Break ended.");
    }

    // ---------------- helpers ----------------

    private async Task EndBreakAsync(CandidateOnboarding c, CandidateWorkSetting s, TimesheetDay day, TimesheetBreak open, int appUserId)
    {
        open.BreakEndAt = DateTime.UtcNow;
        var duration = (int)(open.BreakEndAt.Value - open.BreakStartAt).TotalMinutes;

        var closedBefore = await _db.TimesheetBreaks
            .Where(b => b.TimesheetDayId == day.TimesheetDayId && b.BreakEndAt != null && b.TimesheetBreakId != open.TimesheetBreakId)
            .Select(b => b.BreakEndAt!.Value - b.BreakStartAt)
            .ToListAsync();

        day.BreakMinutes = (int)closedBefore.Sum(t => t.TotalMinutes) + duration;
        day.UpdatedAt = DateTime.UtcNow;

        var overLimit = duration > s.BreakAlertMinutes;
        open.AlertRaised = overLimit;
        if (overLimit)
        {
            await _core.EvaluateFlagsAsync(day, s);
        }

        await _db.SaveChangesAsync();

        if (!overLimit)
        {
            return;
        }

        var name = $"{c.FirstName} {c.LastName}";
        var limitText = s.BreakAlertMinutes % 60 == 0 ? $"{s.BreakAlertMinutes / 60}-hour" : $"{s.BreakAlertMinutes}-minute";
        var message = $"Break logged as {TimeHelper.FormatMinutes(duration)} - over the {limitText} limit.";

        _db.TimesheetAlerts.Add(new TimesheetAlert
        {
            CandidateId = c.CandidateId, TimesheetDayId = day.TimesheetDayId, TimesheetBreakId = open.TimesheetBreakId,
            AlertType = "BreakOverLimit", Message = message, NotifyCandidate = true, NotifyAdmin = true,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        var logId = await _audit.LogAsync(new AuditEntry(
            appUserId, LogCategory.Alerts, Severity.Danger, "break_over_limit", "TimesheetBreak",
            open.TimesheetBreakId, c.CandidateId,
            $"{name} exceeded a {limitText} break on {day.WorkDate:dddd}",
            null));
        await _audit.EmailAsync(appUserId, $"{name} <{c.Email}>", $"Your break exceeded {s.BreakAlertMinutes} minutes on {day.WorkDate:dddd}", logId);
        await _audit.NotifyAsync(appUserId, "Break over the limit", message, "/candidate/dashboard");
        await _audit.NotifyPermissionHoldersAsync(
            PermissionCodes.ApproveTimesheets, c.CandidateId, "Break-time alert", $"{name}: {message}", "/super-admin/activity-log");
    }

    private async Task<(CandidateOnboarding C, CandidateWorkSetting S, TimeZoneInfo Zone, DateOnly Today)> ContextAsync(int appUserId)
    {
        var c = await _core.GetCandidateByUserAsync(appUserId);
        var s = await _core.GetSettingAsync(c.CandidateId);
        var zone = TimeHelper.GetZone(s.TimeZoneId);
        return (c, s, zone, TimeHelper.LocalToday(zone));
    }

    private Task<TimesheetDay?> FindTodayAsync(int candidateId, DateOnly today) =>
        _db.TimesheetDays.FirstOrDefaultAsync(d => d.CandidateId == candidateId && d.WorkDate == today);

    private async Task<TimesheetDay> RequireClockedInAsync(int candidateId, DateOnly today)
    {
        var day = await FindTodayAsync(candidateId, today);
        if (day?.ClockInAt is null || day.ClockOutAt is not null)
        {
            throw new BusinessRuleException("You're not clocked in.");
        }

        return day;
    }

    private Task<TimesheetBreak?> OpenBreakAsync(long dayId) =>
        _db.TimesheetBreaks.FirstOrDefaultAsync(b => b.TimesheetDayId == dayId && b.BreakEndAt == null);

    private async Task<ClockStateDto> BuildStateAsync(TimesheetDay? day, TimeZoneInfo zone, DateOnly today, string? message)
    {
        var state = new ClockStateDto { LocalDate = today.ToString("yyyy-MM-dd"), TimeZoneId = zone.Id, Message = message };
        if (day?.ClockInAt is null)
        {
            return state;
        }

        state.ClockInAt = DateTime.SpecifyKind(day.ClockInAt.Value, DateTimeKind.Utc);
        state.ClockOutAt = day.ClockOutAt is null ? null : DateTime.SpecifyKind(day.ClockOutAt.Value, DateTimeKind.Utc);
        state.BreakMinutes = day.BreakMinutes;
        state.BreakCount = day.BreakCount;
        state.WorkedMinutes = day.WorkedMinutes;

        if (day.ClockOutAt is not null)
        {
            state.Status = "ClockedOut";
            return state;
        }

        var open = await OpenBreakAsync(day.TimesheetDayId);
        state.Status = open is null ? "Working" : "OnBreak";
        state.OnBreakSince = open is null ? null : DateTime.SpecifyKind(open.BreakStartAt, DateTimeKind.Utc);
        return state;
    }
}
