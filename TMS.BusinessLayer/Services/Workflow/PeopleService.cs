using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IPeopleService
{
    Task<List<PersonListItemDto>> ListAsync(int actorUserId, string? search, int? companyId, int? teamId);

    Task<PersonDetailDto> GetAsync(int actorUserId, int candidateId);

    Task<List<StaffMemberDto>> StaffAsync(string? search);
}

public class PeopleService : IPeopleService
{
    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;

    public PeopleService(TmsDbContext db, IScopeService scope)
    {
        _db = db;
        _scope = scope;
    }

    private static string Fmt(DateOnly d) => d.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

    public async Task<List<PersonListItemDto>> ListAsync(int actorUserId, string? search, int? companyId, int? teamId)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var q = _db.CandidateOnboardings.Include(c => c.Team).Include(c => c.Company)
            .Where(c => c.Status == OnboardingStatus.Approved);
        if (allowed is not null) q = q.Where(c => allowed.Contains(c.CandidateId));
        if (companyId is not null) q = q.Where(c => c.CompanyId == companyId);
        if (teamId is not null) q = q.Where(c => c.TeamId == teamId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            q = q.Where(c => (c.FirstName + " " + c.LastName).Contains(t) || (c.Email != null && c.Email.Contains(t)));
        }

        var rows = await q.OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToListAsync();
        return rows.Select(c => new PersonListItemDto
        {
            Id = c.CandidateId.ToString(),
            Name = $"{c.FirstName} {c.LastName}",
            Team = c.Team?.TeamName ?? "—",
            Company = c.Company?.CompanyName ?? "—",
            Since = Fmt(c.JoiningDate ?? DateOnly.FromDateTime(c.CreatedAt)),
            Role = c.RequestType == "Candidate" ? "Candidate" : "Resource Manager",
        }).ToList();
    }

    public async Task<PersonDetailDto> GetAsync(int actorUserId, int candidateId)
    {
        if (!await _scope.CanAccessCandidateAsync(actorUserId, candidateId))
            throw new ForbiddenException("You do not have access to this person.");

        var c = await _db.CandidateOnboardings.Include(x => x.Team).Include(x => x.Company).Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.CandidateId == candidateId)
            ?? throw new NotFoundException("Person not found.");

        var approval = await _db.CandidateApprovals.Include(a => a.Approver)
            .Where(a => a.CandidateId == candidateId && a.Decision == "Approved")
            .OrderByDescending(a => a.DecidedAt).FirstOrDefaultAsync();

        var days = await _db.TimesheetDays.Where(d => d.CandidateId == candidateId && d.Status != DayStatus.Draft)
            .Select(d => new { d.WorkDate, d.Status, d.WorkedMinutes, d.SubmittedAt }).ToListAsync();

        var timeline = days.GroupBy(d => new DateOnly(d.WorkDate.Year, d.WorkDate.Month, 1))
            .OrderByDescending(g => g.Key).Take(6)
            .Select(g =>
            {
                var pending = g.Count(d => d.Status == DayStatus.Pending);
                var last = g.Max(d => d.SubmittedAt);
                return new MonthSubmissionDto
                {
                    Label = g.Key.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                    DaysSubmitted = g.Count(),
                    Hours = TimeHelper.FormatHours(Math.Round(g.Sum(d => d.WorkedMinutes) / 60m, 2)) + " hrs",
                    Approved = g.Count(d => d.Status == DayStatus.Approved),
                    Pending = pending,
                    LastSubmission = last is null ? "—" : last.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
                    Tone = pending > 0 ? "warning" : "success",
                };
            }).ToList();

        return new PersonDetailDto
        {
            Id = c.CandidateId.ToString(),
            Name = $"{c.FirstName} {c.LastName}",
            Email = c.Email ?? c.AppUser?.Email ?? "—",
            Phone = string.IsNullOrWhiteSpace(c.PhoneNumber) ? "—" : $"{c.PhoneDialCode} {c.PhoneNumber}".Trim(),
            Team = c.Team?.TeamName ?? "—",
            Company = c.Company?.CompanyName ?? "—",
            Since = Fmt(c.JoiningDate ?? DateOnly.FromDateTime(c.CreatedAt)),
            DateOfBirth = c.DateOfBirth is null ? "—" : Fmt(c.DateOfBirth.Value),
            OnboardedOn = Fmt(DateOnly.FromDateTime(c.ActivatedAt ?? c.CreatedAt)),
            ApprovedBy = approval?.Approver.FullName ?? "—",
            AccountStatus = c.IsActive ? "Active" : "Inactive",
            Role = c.RequestType == "Candidate" ? "Candidate" : "Resource Manager",
            TimesheetsSummary = $"{days.Count} days submitted · {days.Count(d => d.Status == DayStatus.Approved)} approved",
            Timeline = timeline,
        };
    }

    public async Task<List<StaffMemberDto>> StaffAsync(string? search)
    {
        var q = _db.AppUsers
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Company)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Team)
            .Where(u => u.UserRoles.Any(ur => ur.Role.RoleName != RoleNames.Candidate));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            q = q.Where(u => u.FullName.Contains(t) || u.Email.Contains(t));
        }

        var users = await q.OrderBy(u => u.FullName).ToListAsync();
        return users.Select(u =>
        {
            var ur = u.UserRoles.First(r => r.Role.RoleName != RoleNames.Candidate);
            return new StaffMemberDto
            {
                Id = u.AppUserId.ToString(),
                Name = u.FullName,
                Email = u.Email,
                Role = ur.Role.RoleName,
                Company = ur.Company?.CompanyName ?? "All companies",
                Team = ur.Team?.TeamName ?? "All teams",
                Since = Fmt(DateOnly.FromDateTime(ur.GrantedAt)),
                AccountStatus = u.IsActive ? "Active" : "Inactive",
            };
        }).ToList();
    }
}
