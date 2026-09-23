using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class Company
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? EmailDomain { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CandidateOnboarding> CandidateOnboardings { get; set; } = new List<CandidateOnboarding>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
