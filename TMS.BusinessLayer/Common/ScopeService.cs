using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Context;
using TMS.Utilities.Constants;

namespace TMS.BusinessLayer.Common;

/// <summary>
/// Decides which candidates a staff member may see. A role granted with no company/team
/// (Admin) sees everyone; a Manager scoped to Company X / Team Y only sees that group.
/// </summary>
public interface IScopeService
{
    /// <summary>Null means "unrestricted".</summary>
    Task<HashSet<int>?> AllowedCandidateIdsAsync(int appUserId);

    Task<bool> CanAccessCandidateAsync(int appUserId, int candidateId);
}

public class ScopeService : IScopeService
{
    private readonly TmsDbContext _db;

    public ScopeService(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<HashSet<int>?> AllowedCandidateIdsAsync(int appUserId)
    {
        var grants = await _db.UserRoles
            .Where(ur => ur.AppUserId == appUserId && ur.Role.RoleName != RoleNames.Candidate)
            .Select(ur => new { ur.CompanyId, ur.TeamId })
            .ToListAsync();

        if (grants.Any(g => g.CompanyId == null && g.TeamId == null))
        {
            return null;
        }

        var candidates = await _db.CandidateOnboardings
            .Select(c => new { c.CandidateId, c.CompanyId, c.TeamId })
            .ToListAsync();

        return candidates
            .Where(c => grants.Any(g =>
                (g.CompanyId == null || g.CompanyId == c.CompanyId) &&
                (g.TeamId == null || g.TeamId == c.TeamId)))
            .Select(c => c.CandidateId)
            .ToHashSet();
    }

    public async Task<bool> CanAccessCandidateAsync(int appUserId, int candidateId)
    {
        var allowed = await AllowedCandidateIdsAsync(appUserId);
        return allowed is null || allowed.Contains(candidateId);
    }
}
