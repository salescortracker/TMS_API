using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class ActivityLog
{
    public long ActivityLogId { get; set; }

    public int? ActorAppUserId { get; set; }

    public string ActionType { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public long? EntityId { get; set; }

    public int? TargetCandidateId { get; set; }

    public string Severity { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? DetailsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AppUser? ActorAppUser { get; set; }

    public virtual CandidateOnboarding? TargetCandidate { get; set; }
}
