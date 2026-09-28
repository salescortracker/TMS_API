namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetUploadRow
{
    public long UploadRowId { get; set; }

    public int UploadBatchId { get; set; }

    public int RowNumber { get; set; }

    public string? CandidateEmail { get; set; }

    public DateOnly? WorkDate { get; set; }

    public string? LogInText { get; set; }

    public string? LogOutText { get; set; }

    public int? BreakMinutes { get; set; }

    public string? Task { get; set; }

    public string? Description { get; set; }

    public string ValidationStatus { get; set; } = "Pending";

    public string? ErrorMessage { get; set; }

    public long? TimesheetDayId { get; set; }

    public virtual TimesheetUploadBatch UploadBatch { get; set; } = null!;
}
