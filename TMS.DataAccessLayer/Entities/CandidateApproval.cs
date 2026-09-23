using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class CandidateApproval
{
    public int CandidateApprovalId { get; set; }

    public int CandidateId { get; set; }

    public int ApproverId { get; set; }

    public string Decision { get; set; } = null!;

    public string? Comments { get; set; }

    public DateTime? DecidedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Approver Approver { get; set; } = null!;

    public virtual CandidateOnboarding Candidate { get; set; } = null!;
}
