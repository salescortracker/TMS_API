using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IApprovalService
{
    Task<ApprovalListDto> ListAsync(int actorUserId, int? companyId, int? teamId, string? search);

    Task ApproveAsync(int actorUserId, long dayId);

    Task RejectAsync(int actorUserId, long dayId, string reason);

    Task<ApprovalEntryDto> EditAsync(int actorUserId, long dayId, ApprovalEditDto dto);

    Task<ApproveManyResultDto> ApproveManyAsync(int actorUserId, List<long> ids);
}

public class ApprovalService : IApprovalService
{
    private static readonly string[] Submitted = { DayStatus.Pending, DayStatus.Approved, DayStatus.Rejected };

    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;
    private readonly IAuditService _audit;
    private readonly ITimesheetCore _core;

    public ApprovalService(TmsDbContext db, IScopeService scope, IAuditService audit, ITimesheetCore core)
    {
        _db = db;
        _scope = scope;
        _audit = audit;
        _core = core;
    }

    public async Task<ApprovalListDto> ListAsync(int actorUserId, int? companyId, int? teamId, string? search)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7 * 12);

        var query = _db.TimesheetDays
            .Include(d => d.Candidate).ThenInclude(c => c.Team)
            .Include(d => d.Candidate).ThenInclude(c => c.Company)
            .Where(d => Submitted.Contains(d.Status) && d.WorkDate >= since);

        if (allowed is not null) query = query.Where(d => allowed.Contains(d.CandidateId));
        if (companyId is not null) query = query.Where(d => d.Candidate.CompanyId == companyId);
        if (teamId is not null) query = query.Where(d => d.Candidate.TeamId == teamId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d =>
                (d.Candidate.FirstName + " " + d.Candidate.LastName).Contains(term) ||
                (d.Candidate.Email != null && d.Candidate.Email.Contains(term)));
        }

        var days = await query.OrderByDescending(d => d.SubmittedAt).ThenByDescending(d => d.WorkDate).Take(1000).ToListAsync();
        var candidateIds = days.Select(d => d.CandidateId).Distinct().ToList();
        var weeks = days.Select(d => d.WeekStartDate).Distinct().ToList();

        var weekMinutes = (await _db.TimesheetDays
                .Where(d => candidateIds.Contains(d.CandidateId) && weeks.Contains(d.WeekStartDate))
                .GroupBy(d => new { d.CandidateId, d.WeekStartDate })
                .Select(g => new { g.Key.CandidateId, g.Key.WeekStartDate, Minutes = g.Sum(x => x.WorkedMinutes) })
                .ToListAsync())
            .ToDictionary(x => (x.CandidateId, x.WeekStartDate), x => x.Minutes);

        var settings = await _db.CandidateWorkSettings.Where(s => candidateIds.Contains(s.CandidateId))
            .ToDictionaryAsync(s => s.CandidateId);

        return new ApprovalListDto
        {
            Items = days.Select(d =>
            {
                settings.TryGetValue(d.CandidateId, out var s);
                weekMinutes.TryGetValue((d.CandidateId, d.WeekStartDate), out var wm);
                return ToDto(d, s, wm);
            }).ToList(),
        };
    }

    private static ApprovalEntryDto ToDto(TimesheetDay d, CandidateWorkSetting? s, int weekMinutes)
    {
        var zone = TimeHelper.GetZone(s?.TimeZoneId);
        var c = d.Candidate;
        return new ApprovalEntryDto
        {
            Id = d.TimesheetDayId.ToString(),
            CandidateId = d.CandidateId,
            Candidate = $"{c.FirstName} {c.LastName}",
            Email = c.Email ?? string.Empty,
            Team = c.Team?.TeamName ?? string.Empty,
            Company = c.Company?.CompanyName ?? "Others",
            Day = d.WorkDate.ToString("ddd, MMM d", CultureInfo.InvariantCulture),
            WorkDate = d.WorkDate.ToString("yyyy-MM-dd"),
            TimeRange = $"{TimeHelper.FormatClock(d.ClockInAt, zone)} – {TimeHelper.FormatClock(d.ClockOutAt, zone)}",
            LogIn = TimeHelper.FormatHm(d.ClockInAt, zone),
            LogOut = TimeHelper.FormatHm(d.ClockOutAt, zone),
            BreakMinutes = d.BreakMinutes,
            Task = d.Task ?? string.Empty,
            Description = d.Description ?? string.Empty,
            Hours = TimeHelper.FormatHours(Math.Round(d.WorkedMinutes / 60m, 2)) + " hrs",
            WeekProgress = $"Week {TimeHelper.FormatHours(Math.Round(weekMinutes / 60m, 2))} / {TimeHelper.FormatHours(s?.WeeklyTargetHours ?? 40)}",
            Checks = d.IsFlagged ? "Flagged" : "Clean",
            FlagReason = d.FlagReason,
            Decision = d.Status,
            RejectionReason = d.RejectionReason,
        };
    }

    // ---------------- decisions ----------------

    public async Task ApproveAsync(int actorUserId, long dayId)
    {
        var day = await LoadAsync(actorUserId, dayId);
        if (day.Status != DayStatus.Pending)
        {
            throw new BusinessRuleException($"This day is {day.Status.ToLowerInvariant()}, not waiting for approval.");
        }

        var approverId = await _core.EnsureApproverAsync(actorUserId);
        _core.ChangeStatus(day, DayStatus.Approved, null, actorUserId, null);
        day.ReviewedAt = DateTime.UtcNow;
        day.ReviewedByApproverId = approverId;
        day.ReviewedByAppUserId = actorUserId;
        day.RejectionReason = null;
        await _db.SaveChangesAsync();

        var actor = await ActorNameAsync(actorUserId);
        var name = $"{day.Candidate.FirstName} {day.Candidate.LastName}";
        var logId = await _audit.LogAsync(new AuditEntry(
            actorUserId, LogCategory.Approvals, Severity.Success, "timesheet_approved", "TimesheetDay",
            day.TimesheetDayId, day.CandidateId,
            $"{actor} approved {name}'s timesheet",
            null));
        await _audit.EmailAsync(day.Candidate.AppUserId, $"{name} <{day.Candidate.Email}>",
            $"Your timesheet for {day.WorkDate:ddd MMM d} was approved", logId);
        if (day.Candidate.AppUserId is int uid)
        {
            await _audit.NotifyAsync(uid, "Timesheet approved", $"{day.WorkDate:ddd MMM d} was approved.", "/candidate/my-history");
        }
    }

    public async Task RejectAsync(int actorUserId, long dayId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleException("Give a reason so the candidate knows what to fix.");
        }

        var day = await LoadAsync(actorUserId, dayId);
        if (day.Status != DayStatus.Pending)
        {
            throw new BusinessRuleException($"This day is {day.Status.ToLowerInvariant()}, not waiting for approval.");
        }

        var approverId = await _core.EnsureApproverAsync(actorUserId);
        _core.ChangeStatus(day, DayStatus.Rejected, null, actorUserId, reason.Trim());
        day.ReviewedAt = DateTime.UtcNow;
        day.ReviewedByApproverId = approverId;
        day.ReviewedByAppUserId = actorUserId;
        day.RejectionReason = reason.Trim();
        await _db.SaveChangesAsync();

        var actor = await ActorNameAsync(actorUserId);
        var name = $"{day.Candidate.FirstName} {day.Candidate.LastName}";
        var logId = await _audit.LogAsync(new AuditEntry(
            actorUserId, LogCategory.Approvals, Severity.Danger, "timesheet_rejected", "TimesheetDay",
            day.TimesheetDayId, day.CandidateId,
            $"{actor} rejected {name}'s timesheet for {day.WorkDate:ddd MMM d}",
            reason.Trim()));
        await _audit.EmailAsync(day.Candidate.AppUserId, $"{name} <{day.Candidate.Email}>",
            $"Your timesheet for {day.WorkDate:ddd MMM d} needs a fix", logId);
        if (day.Candidate.AppUserId is int uid)
        {
            await _audit.NotifyAsync(uid, "Timesheet rejected", reason.Trim(), "/candidate/my-timesheet");
        }
    }

    public async Task<ApprovalEntryDto> EditAsync(int actorUserId, long dayId, ApprovalEditDto dto)
    {
        var day = await LoadAsync(actorUserId, dayId);
        if (day.Status is DayStatus.Approved)
        {
            throw new BusinessRuleException("Approved days can't be edited.");
        }

        var s = await _core.GetSettingAsync(day.CandidateId);
        var zone = TimeHelper.GetZone(s.TimeZoneId);

        if (!TimeHelper.TryParseTime(dto.LogIn, out var tin) || !TimeHelper.TryParseTime(dto.LogOut, out var tout))
        {
            throw new BusinessRuleException("Enter valid log-in and log-out times.");
        }

        var newIn = TimeHelper.ToUtc(day.WorkDate, tin, zone);
        var newOut = TimeHelper.ToUtc(day.WorkDate, tout, zone);
        if (newOut <= newIn)
        {
            throw new BusinessRuleException("Log-out must be after log-in.");
        }

        if (dto.BreakMinutes > (int)(newOut - newIn).TotalMinutes)
        {
            throw new BusinessRuleException("Break can't be longer than the time worked.");
        }

        var changes = new List<object>();
        var lines = new List<string>();

        void Track(string field, string label, string before, string after)
        {
            if (before != after)
            {
                changes.Add(new { field, from = before, to = after });
                lines.Add($"{day.WorkDate:dddd} {label} changed from {before} to {after}.");
            }
        }

        Track("logIn", "log-in", TimeHelper.FormatClock(day.ClockInAt, zone), TimeHelper.FormatClock(newIn, zone));
        Track("logOut", "log-out", TimeHelper.FormatClock(day.ClockOutAt, zone), TimeHelper.FormatClock(newOut, zone));
        Track("break", "break", $"{day.BreakMinutes} min", $"{dto.BreakMinutes} min");

        _core.ApplyTimes(day, newIn, newOut, dto.BreakMinutes);
        await _core.EnsureWeekCapAsync(day.CandidateId, day.WeekStartDate, day.TimesheetDayId, day.WorkedMinutes, s);

        if (dto.Task is not null) day.Task = dto.Task.Trim();
        if (dto.Description is not null) day.Description = dto.Description.Trim();
        await _core.EvaluateFlagsAsync(day, s);
        day.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (changes.Count > 0)
        {
            var actor = await ActorNameAsync(actorUserId);
            var name = $"{day.Candidate.FirstName} {day.Candidate.LastName}";
            await _audit.LogAsync(new AuditEntry(
                actorUserId, LogCategory.EditsRoles, Severity.Warning, "timesheet_edited", "TimesheetDay",
                day.TimesheetDayId, day.CandidateId,
                $"{actor} edited {name}'s timesheet",
                JsonSerializer.Serialize(new { summary = string.Join(" ", lines), changes })));
            if (day.Candidate.AppUserId is int uid)
            {
                await _audit.NotifyAsync(uid, "Your timesheet was edited", string.Join(" ", lines), "/candidate/my-timesheet");
            }
        }

        var weekMinutes = await _db.TimesheetDays
            .Where(d => d.CandidateId == day.CandidateId && d.WeekStartDate == day.WeekStartDate)
            .SumAsync(d => d.WorkedMinutes);
        await _db.Entry(day.Candidate).Reference(c => c.Team).LoadAsync();
        await _db.Entry(day.Candidate).Reference(c => c.Company).LoadAsync();
        return ToDto(day, s, weekMinutes);
    }

    public async Task<ApproveManyResultDto> ApproveManyAsync(int actorUserId, List<long> ids)
    {
        var result = new ApproveManyResultDto();
        foreach (var id in ids.Distinct())
        {
            try
            {
                await ApproveAsync(actorUserId, id);
                result.Approved++;
            }
            catch (Exception ex) when (ex is BusinessRuleException or ForbiddenException or NotFoundException)
            {
                result.Skipped.Add($"#{id}: {ex.Message}");
            }
        }

        return result;
    }

    // ---------------- helpers ----------------

    private async Task<TimesheetDay> LoadAsync(int actorUserId, long dayId)
    {
        var day = await _db.TimesheetDays.Include(d => d.Candidate)
                      .FirstOrDefaultAsync(d => d.TimesheetDayId == dayId)
                  ?? throw new NotFoundException("Timesheet day not found.");

        if (!await _scope.CanAccessCandidateAsync(actorUserId, day.CandidateId))
        {
            throw new ForbiddenException("This candidate is outside your company/team scope.");
        }

        return day;
    }

    private Task<string> ActorNameAsync(int actorUserId) =>
        _db.AppUsers.Where(u => u.AppUserId == actorUserId).Select(u => u.FullName).FirstAsync();
}
