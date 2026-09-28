using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs.Workflow;

public class LookupItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Extra { get; set; }
}

public class LookupsDto
{
    public List<LookupItemDto> Companies { get; set; } = new();

    public List<LookupItemDto> Teams { get; set; } = new();
}

/// <summary>Candidate self-onboarding form (public). SubmitForApproval=false saves a draft.</summary>
public class OnboardingSubmitDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public int? TeamId { get; set; }

    public int? CompanyId { get; set; }

    [MaxLength(6)]
    public string? PhoneDialCode { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = null!;

    public bool SubmitForApproval { get; set; } = true;
}

/// <summary>Resource Manager self-registration (public).</summary>
public class ManagerRegistrationDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    public int? TeamId { get; set; }

    [Required]
    public int? CompanyId { get; set; }

    [Required, MaxLength(6)]
    public string PhoneDialCode { get; set; } = null!;

    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = null!;

    public bool SubmitForApproval { get; set; } = true;
}

public class OnboardingResultDto
{
    public int CandidateId { get; set; }

    public string Status { get; set; } = null!;

    public string Message { get; set; } = null!;
}

public class SeatInfoDto
{
    public int Used { get; set; }

    public int Total { get; set; }
}

public class ApplicantDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Team { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string Submitted { get; set; } = null!;

    public string? Flag { get; set; }

    public string Status { get; set; } = null!;
}

public class ApplicantListDto
{
    public List<ApplicantDto> Items { get; set; } = new();

    public int SeatsUsed { get; set; }

    public int SeatsTotal { get; set; }
}

public class RejectRequestDto
{
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class ActivationResultDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TemporaryPassword { get; set; }

    public string Message { get; set; } = null!;
}

public class BulkActivationResultDto
{
    public int Approved { get; set; }

    public int SkippedFlagged { get; set; }

    public List<ActivationResultDto> Results { get; set; } = new();
}
