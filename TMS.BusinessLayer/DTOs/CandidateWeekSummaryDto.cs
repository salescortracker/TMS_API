namespace TMS.BusinessLayer.DTOs;

public class CandidateWeekSummaryDto
{
    public int CandidateId { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public int? SubmittedThisWeek { get; set; }

    public int? PendingApproval { get; set; }

    public int? Approved { get; set; }

    public int? Rejected { get; set; }

    public int? Drafts { get; set; }

    public decimal? WorkedHours { get; set; }

    public decimal TargetHours { get; set; }
}
