using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IActivityService
{
    Task<List<ActivityEntryDto>> ListAsync(int actorUserId, string? category, string? search, int take);

    Task<List<EmailAlertDto>> EmailsAsync(int take);

    Task<NotificationsDto> NotificationsAsync(int appUserId);

    Task MarkReadAsync(int appUserId, long? notificationId);
}

public class ActivityService : IActivityService
{
    private readonly TmsDbContext _db;
    private readonly IScopeService _scope;

    public ActivityService(TmsDbContext db, IScopeService scope)
    {
        _db = db;
        _scope = scope;
    }

    private static string Iso(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc).ToString("o");

    public async Task<List<ActivityEntryDto>> ListAsync(int actorUserId, string? category, string? search, int take)
    {
        var allowed = await _scope.AllowedCandidateIdsAsync(actorUserId);
        var q = _db.ActivityLogs.Include(l => l.ActorAppUser).Include(l => l.TargetCandidate).AsQueryable();
        if (allowed is not null)
            q = q.Where(l => l.TargetCandidateId == null || allowed.Contains(l.TargetCandidateId.Value));
        if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
            q = q.Where(l => l.Category == category);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            q = q.Where(l => l.Description.Contains(t));
        }

        var logs = await q.OrderByDescending(l => l.CreatedAt).Take(Math.Clamp(take, 1, 500)).ToListAsync();
        return logs.Select(l =>
        {
            var (actor, action) = ActivityMapper.Split(l);
            return new ActivityEntryDto
            {
                Id = l.ActivityLogId,
                Actor = actor,
                Action = action,
                Description = ActivityMapper.SummaryOf(l) ?? string.Empty,
                Category = l.Category,
                Tone = ActivityMapper.ToneOf(l.Severity, l.ActionType),
                Time = Iso(l.CreatedAt),
            };
        }).ToList();
    }

    public async Task<List<EmailAlertDto>> EmailsAsync(int take)
    {
        var rows = await _db.EmailAlerts.OrderByDescending(e => e.SentAt).Take(Math.Clamp(take, 1, 200)).ToListAsync();
        return rows.Select(e => new EmailAlertDto
        {
            Id = e.EmailAlertId, Recipient = e.RecipientLabel, Subject = e.Subject, Time = Iso(e.SentAt),
        }).ToList();
    }

    public async Task<NotificationsDto> NotificationsAsync(int appUserId)
    {
        var rows = await _db.Notifications.Where(n => n.AppUserId == appUserId)
            .OrderByDescending(n => n.CreatedAt).Take(30).ToListAsync();
        return new NotificationsDto
        {
            Unread = await _db.Notifications.CountAsync(n => n.AppUserId == appUserId && !n.IsRead),
            Items = rows.Select(n => new NotificationDto
            {
                Id = n.NotificationId, Title = n.Title, Message = n.Message, LinkUrl = n.LinkUrl,
                IsRead = n.IsRead, Time = Iso(n.CreatedAt),
            }).ToList(),
        };
    }

    public async Task MarkReadAsync(int appUserId, long? notificationId)
    {
        var q = _db.Notifications.Where(n => n.AppUserId == appUserId && !n.IsRead);
        if (notificationId is not null) q = q.Where(n => n.NotificationId == notificationId);
        foreach (var n in await q.ToListAsync()) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
