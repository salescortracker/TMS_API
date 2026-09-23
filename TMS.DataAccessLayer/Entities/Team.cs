using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class Team
{
    public int TeamId { get; set; }

    public string TeamName { get; set; } = null!;

    public string DialCode { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CandidateOnboarding> CandidateOnboardings { get; set; } = new List<CandidateOnboarding>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
