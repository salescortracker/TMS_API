using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class Approver
{
    public int ApproverId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? RoleLabel { get; set; }

    public byte ApprovalOrder { get; set; }

    public bool IsActive { get; set; }

    public int? AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    public virtual ICollection<CandidateApproval> CandidateApprovals { get; set; } = new List<CandidateApproval>();

    public virtual ICollection<TimesheetDay> TimesheetDays { get; set; } = new List<TimesheetDay>();

    public virtual ICollection<TimesheetStatusHistory> TimesheetStatusHistories { get; set; } = new List<TimesheetStatusHistory>();
}
