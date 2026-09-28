using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;

namespace TMS.BusinessLayer.Common;

public record AuditEntry(
    int? ActorAppUserId,
    string Category,
    string Severity,
    string ActionType,
    string EntityType,
    long? EntityId,
    int? TargetCandidateId,
    string Description,
    string? DetailsJson = null);

/// <summary>Writes the Activity Log, simulated email alerts and in-app notifications.</summary>
public interface IAuditService
{
    Task<long> LogAsync(AuditEntry entry);

    Task EmailAsync(int? recipientUserId, string recipientLabel, string subject, long? activityLogId);

    Task NotifyAsync(int appUserId, string title, string? message = null, string? link = null);

    /// <summary>Notifies everyone holding a permission whose scope covers the candidate.</summary>
    Task NotifyPermissionHoldersAsync(string permissionCode, int? candidateId, string title, string? message = null, string? link = null);
}

public class AuditService : IAuditService
{
    private readonly TmsDbContext _db;

    public AuditService(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<long> LogAsync(AuditEntry e)
    {
        var log = new ActivityLog
        {
            ActorAppUserId = e.ActorAppUserId,
            Category = e.Category,
            Severity = e.Severity,
            ActionType = e.ActionType,
            EntityType = e.EntityType,
            EntityId = e.EntityId,
            TargetCandidateId = e.TargetCandidateId,
            Description = Truncate(e.Description, 500),
            DetailsJson = e.DetailsJson,
            CreatedAt = DateTime.UtcNow,
        };
        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync();
        return log.ActivityLogId;
    }

    public async Task EmailAsync(int? recipientUserId, string recipientLabel, string subject, long? activityLogId)
    {
        _db.EmailAlerts.Add(new EmailAlert
        {
            RecipientAppUserId = recipientUserId,
            RecipientLabel = Truncate(recipientLabel, 200),
            Subject = Truncate(subject, 200),
            ActivityLogId = activityLogId,
            SentAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyAsync(int appUserId, string title, string? message = null, string? link = null)
    {
        _db.Notifications.Add(new Notification
        {
            AppUserId = appUserId,
            Title = Truncate(title, 200),
            Message = message is null ? null : Truncate(message, 500),
            LinkUrl = link,
            CreatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyPermissionHoldersAsync(
        string permissionCode, int? candidateId, string title, string? message = null, string? link = null)
    {
        var grants = await _db.UserRoles
            .Where(ur => ur.Role.Permissions.Any(p => p.PermissionCode == permissionCode) && ur.AppUser.IsActive)
            .Select(ur => new { ur.AppUserId, ur.CompanyId, ur.TeamId })
            .ToListAsync();

        int? companyId = null;
        int? teamId = null;
        if (candidateId is not null)
        {
            var c = await _db.CandidateOnboardings
                .Where(x => x.CandidateId == candidateId)
                .Select(x => new { x.CompanyId, x.TeamId })
                .FirstOrDefaultAsync();
            companyId = c?.CompanyId;
            teamId = c?.TeamId;
        }

        var userIds = grants
            .Where(g => candidateId is null ||
                        ((g.CompanyId == null || g.CompanyId == companyId) &&
                         (g.TeamId == null || g.TeamId == teamId)))
            .Select(g => g.AppUserId)
            .Distinct()
            .ToList();

        foreach (var userId in userIds)
        {
            _db.Notifications.Add(new Notification
            {
                AppUserId = userId,
                Title = Truncate(title, 200),
                Message = message is null ? null : Truncate(message, 500),
                LinkUrl = link,
                CreatedAt = DateTime.UtcNow,
            });
        }

        if (userIds.Count > 0)
        {
            await _db.SaveChangesAsync();
        }
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
