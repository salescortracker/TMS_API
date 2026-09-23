using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class CandidateOnboardingDto
{
    public int CandidateId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public int? TeamId { get; set; }

    public int? CompanyId { get; set; }

    [MaxLength(6)]
    public string? PhoneDialCode { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(254)]
    public string? Email { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = null!;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ActivatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? AppUserId { get; set; }

    public bool IsActive { get; set; }
}
