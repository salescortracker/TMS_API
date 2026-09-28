using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.Utilities.Constants;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(int actorUserId, int? companyId, int? teamId);
}

public class DashboardService : IDashboardService
{
    private static readonly string[] SubmittedStatuses = { DayStatus.Pending, DayStatus.Approved, DayStatus.Rejected };

    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;
    private readonly ITimesheetCore _core;

    public DashboardService(TmsDbContext db, IScopeService scope, ITimesheetCore core)
    {
        _db = db;
        _scope = scope;
        _core = core;
    }

    public async Task<DashboardDto> GetAsync(int actorUserId, int? companyId, int? teamId)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = TimeHelper.WeekStart(today);

        var cq = _db.CandidateOnboardings.Include(c => c.Team).Include(c => c.Company)
            .Where(c => c.RequestType == "Candidate" && c.Status == OnboardingStatus.Approved && c.IsActive && c.AppUserId != null);
        if (allowed is not null) cq = cq.Where(c => allowed.Contains(c.CandidateId));
        if (companyId is not null) cq = cq.Where(c => c.CompanyId == companyId);
        if (teamId is not null) cq = cq.Where(c => c.TeamId == teamId);
        var candidates = await cq.ToListAsync();
        var ids = candidates.Select(c => c.CandidateId).ToList();

        var allDays = await _db.TimesheetDays
            .Where(d => ids.Contains(d.CandidateId) && (d.WeekStartDate == weekStart || d.Status == DayStatus.Pending || d.Status == DayStatus.Rejected))
            .ToListAsync();
        var weekDays = allDays.Where(d => d.WeekStartDate == weekStart).ToList();
        var todayDays = allDays.Where(d => d.WorkDate == today && d.ClockInAt != null).ToList();
        var todayIds = todayDays.Select(d => d.TimesheetDayId).ToList();
        var openBreaks = await _db.TimesheetBreaks
            .Where(b => todayIds.Contains(b.TimesheetDayId) && b.BreakEndAt == null)
            .ToListAsync();

        // The weekday we report "not submitted" for: today, or the last weekday on weekends.
        var reportDate = today;
        while (reportDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) reportDate = reportDate.AddDays(-1);
        var submittedOnDate = (await _db.TimesheetDays
                .Where(d => ids.Contains(d.CandidateId) && d.WorkDate == reportDate && SubmittedStatuses.Contains(d.Status))
                .Select(d => d.CandidateId).ToListAsync()).ToHashSet();
        var notSubmittedIds = ids.Where(id => !submittedOnDate.Contains(id)).ToHashSet();

        var pendingQuery = _db.CandidateOnboardings.Where(c => c.Status == OnboardingStatus.Pending);
        if (allowed is not null) pendingQuery = pendingQuery.Where(c => allowed.Contains(c.CandidateId));
        var onboardingPending = await pendingQuery.CountAsync();
        var alerts = await _db.TimesheetAlerts.CountAsync(a =>
            a.AlertType == "BreakOverLimit" && a.CreatedAt >= DateTime.UtcNow.AddDays(-14) && ids.Contains(a.CandidateId));

        var clockedIn = todayDays.Count(d => d.ClockOutAt == null);
        var onBreak = openBreaks.Count;

        var dto = new DashboardDto
        {
            WeekLabel = $"{weekStart:MMM d} – {weekStart.AddDays(6):MMM d, yyyy}",
            ScopeLabel = (companyId, teamId) switch
            {
                (null, null) => "All companies and teams",
                _ => "Filtered view",
            },
        };

        dto.Stats = new List<StatCardDto>
        {
            new() { Label = "Days submitted this week", Value = weekDays.Count(d => SubmittedStatuses.Contains(d.Status)) },
            new() { Label = "Pending approval", Value = allDays.Count(d => d.Status == DayStatus.Pending), Tone = "warning", Highlight = true },
            new() { Label = "Approved this week", Value = weekDays.Count(d => d.Status == DayStatus.Approved), Tone = "success" },
            new() { Label = "Rejected — needs a fix", Value = allDays.Count(d => d.Status == DayStatus.Rejected), Tone = "danger" },
            new() { Label = $"Not submitted for {reportDate:dddd, MMM d}", Value = notSubmittedIds.Count },
            new() { Label = "Onboarding pending", Value = onboardingPending },
            new() { Label = "Break-time alerts (2 weeks)", Value = alerts, Tone = "danger" },
            new() { Label = "Active candidates", Value = candidates.Count },
            new() { Label = "Clocked in now", Value = clockedIn, Tone = "success" },
            new() { Label = "On break now", Value = onBreak, Tone = "warning" },
        };

        dto.TeamRows = candidates
            .GroupBy(c => new { Company = c.Company?.CompanyName ?? "Others", Team = c.Team?.TeamName ?? "—" })
            .OrderBy(g => g.Key.Company).ThenBy(g => g.Key.Team)
            .Select(g =>
            {
                var gids = g.Select(c => c.CandidateId).ToHashSet();
                var gWeek = weekDays.Where(d => gids.Contains(d.CandidateId)).ToList();
                return new CompanyTeamRowDto
                {
                    Name = $"{g.Key.Company} · {g.Key.Team}",
                    Candidates = gids.Count,
                    ClockedIn = todayDays.Count(d => gids.Contains(d.CandidateId) && d.ClockOutAt == null),
                    Submitted = gWeek.Count(d => SubmittedStatuses.Contains(d.Status)),
                    Pending = allDays.Count(d => gids.Contains(d.CandidateId) && d.Status == DayStatus.Pending),
                    Approved = gWeek.Count(d => d.Status == DayStatus.Approved),
                    Rejected = allDays.Count(d => gids.Contains(d.CandidateId) && d.Status == DayStatus.Rejected),
                    NotSubmitted = gids.Count(id => notSubmittedIds.Contains(id)),
                };
            }).ToList();

        var byId = candidates.ToDictionary(c => c.CandidateId);
        var settings = await _db.CandidateWorkSettings.Where(s => ids.Contains(s.CandidateId)).ToDictionaryAsync(s => s.CandidateId);
        dto.Attendance = todayDays.Select(d =>
        {
            var c = byId[d.CandidateId];
            settings.TryGetValue(d.CandidateId, out var s);
            var zone = TimeHelper.GetZone(s?.TimeZoneId);
            var openBreak = openBreaks.FirstOrDefault(b => b.TimesheetDayId == d.TimesheetDayId);
            var breakMinutes = d.BreakMinutes + (openBreak is null ? 0 : (int)(DateTime.UtcNow - openBreak.BreakStartAt).TotalMinutes);
            var limit = s?.BreakAlertMinutes ?? 60;
            var over = breakMinutes > limit;
            var status = d.ClockOutAt is not null ? "Clocked out" : openBreak is not null ? "On break" : "Working";
            var range = d.ClockOutAt is null
                ? $"In {TimeHelper.FormatClock(d.ClockInAt, zone)}"
                : $"In {TimeHelper.FormatClock(d.ClockInAt, zone)} · out {TimeHelper.FormatClock(d.ClockOutAt, zone)}";
            return new AttendanceRowDto
            {
                Name = $"{c.FirstName} {c.LastName}",
                Team = $"{c.Company?.CompanyName ?? "Others"} · {c.Team?.TeamName}",
                TimeRange = range,
                BreakLabel = over ? $"Break {TimeHelper.FormatMinutes(breakMinutes)} — over {limit / 60} hour" : $"Break {TimeHelper.FormatMinutes(breakMinutes)}",
                BreakAlert = over,
                Status = status,
                StatusTone = status switch { "Working" => "success", "On break" => "warning", _ => "neutral" },
            };
        }).OrderBy(a => a.Status == "On break" ? 0 : a.Status == "Working" ? 1 : 2).ThenBy(a => a.Name).ToList();

        for (var i = 0; i < 5; i++)
        {
            var date = weekStart.AddDays(i);
            var forDay = weekDays.Where(d => d.WorkDate == date).ToList();
            var approved = forDay.Count(d => d.Status == DayStatus.Approved);
            var pending = forDay.Count(d => d.Status == DayStatus.Pending);
            var rejected = forDay.Count(d => d.Status == DayStatus.Rejected);
            var submitted = approved + pending + rejected;
            var future = date > today;
            dto.WeekSegments.Add(new DaySegmentDto
            {
                Day = date.ToString("ddd", CultureInfo.InvariantCulture),
                Total = future ? null : submitted,
                Approved = approved,
                Pending = pending,
                Rejected = rejected,
                NotSubmitted = Math.Max(0, candidates.Count - submitted),
                Note = future ? "upcoming" : pending > 0 ? $"{pending} pending" : rejected > 0 ? $"{rejected} rejected" : "all clear",
            });
        }

        dto.Waiting = allDays.Where(d => d.Status == DayStatus.Pending)
            .GroupBy(d => d.CandidateId)
            .Select(g =>
            {
                var oldest = g.Min(d => d.SubmittedAt ?? d.CreatedAt);
                var c = byId[g.Key];
                return new WaitingItemDto
                {
                    Name = $"{c.FirstName} {c.LastName}",
                    Since = oldest.ToString("MMM d", CultureInfo.InvariantCulture),
                    Days = Math.Max(0, (int)(DateTime.UtcNow - oldest).TotalDays),
                };
            })
            .OrderByDescending(w => w.Days).Take(6).ToList();

        var logQuery = _db.ActivityLogs.Include(l => l.ActorAppUser).Include(l => l.TargetCandidate).AsQueryable();
        if (allowed is not null)
        {
            logQuery = logQuery.Where(l => l.TargetCandidateId == null || allowed.Contains(l.TargetCandidateId.Value));
        }

        var logs = await logQuery.OrderByDescending(l => l.CreatedAt).Take(7).ToListAsync();
        dto.Recent = logs.Select(ActivityMapper.ToItem).ToList();

        return dto;
    }
}

/// <summary>Turns an ActivityLog row into the "Actor did something" shape the UI shows.</summary>
public static class ActivityMapper
{
    public static ActivityItemDto ToItem(TMS.DataAccessLayer.Entities.ActivityLog l)
    {
        var (actor, action) = Split(l);
        return new ActivityItemDto
        {
            Actor = actor,
            Action = action,
            Description = SummaryOf(l),
            Category = l.Category,
            Time = DateTime.SpecifyKind(l.CreatedAt, DateTimeKind.Utc).ToString("o"),
            Tone = ToneOf(l.Severity, l.ActionType),
        };
    }

    public static (string Actor, string Action) Split(TMS.DataAccessLayer.Entities.ActivityLog l)
    {
        var text = l.Description;
        var candidates = new[]
        {
            l.ActorAppUser?.FullName,
            l.TargetCandidate is null ? null : $"{l.TargetCandidate.FirstName} {l.TargetCandidate.LastName}",
        };

        foreach (var name in candidates)
        {
            if (!string.IsNullOrEmpty(name) && text.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            {
                return (name, text[name.Length..].TrimStart());
            }
        }

        return (string.Empty, text);
    }

    public static string? SummaryOf(TMS.DataAccessLayer.Entities.ActivityLog l)
    {
        if (string.IsNullOrWhiteSpace(l.DetailsJson))
        {
            return null;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(l.DetailsJson);
            if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("summary", out var summary))
            {
                return summary.GetString();
            }
        }
        catch (System.Text.Json.JsonException)
        {
            // plain-text detail
        }

        return l.DetailsJson;
    }

    public static string ToneOf(string severity, string actionType) => severity switch
    {
        Severity.Danger => "danger",
        Severity.Warning => "warning",
        Severity.Success => "success",
        _ => "neutral",
    };
}
