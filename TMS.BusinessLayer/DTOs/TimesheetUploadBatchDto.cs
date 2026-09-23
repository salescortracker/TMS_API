using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class TimesheetUploadBatchDto
{
    public int UploadBatchId { get; set; }

    public int CandidateId { get; set; }

    [Required]
    [MaxLength(260)]
    public string FileName { get; set; } = null!;

    public int RowCountTotal { get; set; }

    public int RowCountImported { get; set; }

    [Required]
    [MaxLength(15)]
    public string Status { get; set; } = null!;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public DateTime UploadedAt { get; set; }
}
