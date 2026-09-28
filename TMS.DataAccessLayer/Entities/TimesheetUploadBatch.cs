using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class TimesheetUploadBatch
{
    public int UploadBatchId { get; set; }

    public int? CandidateId { get; set; }

    public int? UploadedByAppUserId { get; set; }

    public string FileName { get; set; } = null!;

    public int RowCountTotal { get; set; }

    public int RowCountImported { get; set; }

    public int RowCountError { get; set; }

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual CandidateOnboarding? Candidate { get; set; }

    public virtual ICollection<TimesheetDay> TimesheetDays { get; set; } = new List<TimesheetDay>();

    public virtual ICollection<TimesheetUploadRow> Rows { get; set; } = new List<TimesheetUploadRow>();
}
