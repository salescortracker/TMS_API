using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class AppUser
{
    public int AppUserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public byte[]? PasswordHash { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Approver> Approvers { get; set; } = new List<Approver>();

    public virtual ICollection<CandidateOnboarding> CandidateOnboardings { get; set; } = new List<CandidateOnboarding>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
