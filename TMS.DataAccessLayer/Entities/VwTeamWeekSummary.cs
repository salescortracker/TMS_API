namespace TMS.DataAccessLayer.Entities;

public partial class VwTeamWeekSummary
{
    public int? CompanyId { get; set; }

    public int? TeamId { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public int Candidates { get; set; }

    public decimal? Hours { get; set; }

    public int? Submitted { get; set; }

    public int? Pending { get; set; }

    public int? Approved { get; set; }

    public int? Rejected { get; set; }
}
