using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class CompanyDto
{
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(150)]
    public string CompanyName { get; set; } = null!;

    [MaxLength(100)]
    public string? EmailDomain { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
