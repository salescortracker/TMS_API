using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class CandidateApprovalDto
{
    public int CandidateApprovalId { get; set; }

    public int CandidateId { get; set; }

    public int ApproverId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Decision { get; set; } = null!;

    [MaxLength(500)]
    public string? Comments { get; set; }

    public DateTime? DecidedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
