using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.Utilities.Constants;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IReportService
{
    Task<TimesheetReportDto> GetAsync(int actorUserId, string? mode, string? period, int? companyId, int? teamId, string? search);

    Task<byte[]> ExportAsync(int actorUserId, string? mode, string? period, int? companyId, int? teamId, string? search);
}

public class ReportService : IReportService
{
    private static readonly string[] SubmittedStatuses = { DayStatus.Pending, DayStatus.Approved, DayStatus.Rejected };

    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;

    public ReportService(TmsDbContext db, IScopeService scope)
    {
        _db = db;
        _scope = scope;
    }

    public async Task<TimesheetReportDto> GetAsync(int actorUserId, string? mode, string? period, int? companyId, int? teamId, string? search)
    {
        var monthly = string.Equals(mode, "monthly", StringComparison.OrdinalIgnoreCase);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly from, to;
        string value, label;
        if (monthly)
        {
            var first = period is not null && DateOnly.TryParseExact(period + "-01", "yyyy-MM-dd", out var p)
                ? p : new DateOnly(today.Year, today.Month, 1);
            from = first;
            to = first.AddMonths(1).AddDays(-1);
            value = first.ToString("yyyy-MM");
            label = first.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }
        else
        {
            var ws = TimeHelper.WeekStart(period is not null && DateOnly.TryParse(period, out var p) ? p : today);
            from = ws;
            to = ws.AddDays(6);
            value = ws.ToString("yyyy-MM-dd");
            label = $"{ws:MMM d} – {to:MMM d, yyyy}";
        }

        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var cq = _db.CandidateOnboardings.Include(c => c.Team).Include(c => c.Company)
            .Where(c => c.RequestType == "Candidate" && c.Status == OnboardingStatus.Approved && c.IsActive);
        if (allowed is not null) cq = cq.Where(c => allowed.Contains(c.CandidateId));
        if (companyId is not null) cq = cq.Where(c => c.CompanyId == companyId);
        if (teamId is not null) cq = cq.Where(c => c.TeamId == teamId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            cq = cq.Where(c => (c.FirstName + " " + c.LastName).Contains(term) ||
                               (c.Company != null && c.Company.CompanyName.Contains(term)));
        }

        var candidates = await cq.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToListAsync();
        var ids = candidates.Select(c => c.CandidateId).ToList();
        var days = await _db.TimesheetDays
            .Where(d => ids.Contains(d.CandidateId) && d.WorkDate >= from && d.WorkDate <= to)
            .Select(d => new { d.CandidateId, d.Status, d.WorkedMinutes })
            .ToListAsync();

        var perCandidate = candidates.Select(c =>
        {
            var mine = days.Where(d => d.CandidateId == c.CandidateId).ToList();
            return new
            {
                Candidate = c,
                Minutes = mine.Sum(d => d.WorkedMinutes),
                Submitted = mine.Count(d => SubmittedStatuses.Contains(d.Status)),
                Approved = mine.Count(d => d.Status == DayStatus.Approved),
                Pending = mine.Count(d => d.Status == DayStatus.Pending),
                Rejected = mine.Count(d => d.Status == DayStatus.Rejected),
            };
        }).ToList();

        var dto = new TimesheetReportDto
        {
            Mode = monthly ? "monthly" : "weekly",
            PeriodLabel = label,
            PeriodValue = value,
            TotalSubmitted = perCandidate.Sum(x => x.Submitted),
            TotalApproved = perCandidate.Sum(x => x.Approved),
            TotalPending = perCandidate.Sum(x => x.Pending),
            TotalRejected = perCandidate.Sum(x => x.Rejected),
            Periods = BuildPeriods(monthly, today),
        };

        dto.TeamRows = perCandidate
            .GroupBy(x => $"{x.Candidate.Company?.CompanyName ?? "Others"} · {x.Candidate.Team?.TeamName ?? "—"}")
            .OrderBy(g => g.Key)
            .Select(g => new TeamSummaryRowDto
            {
                Name = g.Key,
                Candidates = g.Count(),
                Hours = TimeHelper.FormatHours(Math.Round(g.Sum(x => x.Minutes) / 60m, 2)) + " hrs",
                Submitted = g.Sum(x => x.Submitted),
                Approved = g.Sum(x => x.Approved),
                Pending = g.Sum(x => x.Pending),
                Rejected = g.Sum(x => x.Rejected),
            }).ToList();

        dto.Candidates = perCandidate.Select(x => new CandidateSummaryRowDto
        {
            CandidateId = x.Candidate.CandidateId,
            Name = $"{x.Candidate.FirstName} {x.Candidate.LastName}",
            Team = $"{x.Candidate.Company?.CompanyName ?? "Others"} · {x.Candidate.Team?.TeamName ?? "—"}",
            WeekHours = TimeHelper.FormatHours(Math.Round(x.Minutes / 60m, 2)) + " hrs",
            Submitted = x.Submitted,
            Approved = x.Approved,
            Pending = x.Pending,
            Rejected = x.Rejected,
        }).ToList();

        return dto;
    }

    private static List<PeriodOptionDto> BuildPeriods(bool monthly, DateOnly today)
    {
        var list = new List<PeriodOptionDto>();
        if (monthly)
        {
            var first = new DateOnly(today.Year, today.Month, 1);
            for (var i = 0; i < 12; i++)
            {
                var m = first.AddMonths(-i);
                list.Add(new PeriodOptionDto { Value = m.ToString("yyyy-MM"), Label = m.ToString("MMMM yyyy", CultureInfo.InvariantCulture) });
            }
        }
        else
        {
            var ws = TimeHelper.WeekStart(today);
            for (var i = 0; i < 12; i++)
            {
                var w = ws.AddDays(-7 * i);
                list.Add(new PeriodOptionDto { Value = w.ToString("yyyy-MM-dd"), Label = $"{w:MMM d} – {w.AddDays(6):MMM d, yyyy}" });
            }
        }

        return list;
    }

    public async Task<byte[]> ExportAsync(int actorUserId, string? mode, string? period, int? companyId, int? teamId, string? search)
    {
        var report = await GetAsync(actorUserId, mode, period, companyId, teamId, search);

        using var wb = new XLWorkbook();
        var teams = wb.AddWorksheet("By company and team");
        teams.Cell(1, 1).Value = $"Timesheets — {report.PeriodLabel}";
        string[] th = { "Company · Team", "Candidates", "Hours", "Submitted", "Approved", "Pending", "Rejected" };
        for (var i = 0; i < th.Length; i++) teams.Cell(3, i + 1).Value = th[i];
        var r = 4;
        foreach (var t in report.TeamRows)
        {
            teams.Cell(r, 1).Value = t.Name;
            teams.Cell(r, 2).Value = t.Candidates;
            teams.Cell(r, 3).Value = t.Hours;
            teams.Cell(r, 4).Value = t.Submitted;
            teams.Cell(r, 5).Value = t.Approved;
            teams.Cell(r, 6).Value = t.Pending;
            teams.Cell(r, 7).Value = t.Rejected;
            r++;
        }

        var people = wb.AddWorksheet("Candidates");
        string[] ph = { "Candidate", "Company · Team", "Hours", "Submitted", "Approved", "Pending", "Rejected" };
        for (var i = 0; i < ph.Length; i++) people.Cell(1, i + 1).Value = ph[i];
        r = 2;
        foreach (var c in report.Candidates)
        {
            people.Cell(r, 1).Value = c.Name;
            people.Cell(r, 2).Value = c.Team;
            people.Cell(r, 3).Value = c.WeekHours;
            people.Cell(r, 4).Value = c.Submitted;
            people.Cell(r, 5).Value = c.Approved;
            people.Cell(r, 6).Value = c.Pending;
            people.Cell(r, 7).Value = c.Rejected;
            r++;
        }

        teams.Columns().AdjustToContents();
        people.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
