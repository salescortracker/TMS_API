using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs.Workflow;

// ---------- Roles & access ----------

public class MenuItemDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class PermissionItemDto
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
}

public class RoleSummaryDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
    public int People { get; set; }
    public List<string> Menus { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class RolesOverviewDto
{
    public List<RoleSummaryDto> Roles { get; set; } = new();
    public List<MenuItemDto> AllMenus { get; set; } = new();
    public List<PermissionItemDto> AllPermissions { get; set; } = new();
}

public class RoleSaveDto
{
    [Required, MaxLength(60)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    public List<string> Menus { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
}

public class StaffUserDto
{
    public int AppUserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string Scope { get; set; } = "All companies & teams";
    public string Status { get; set; } = null!;
    public string LastLogin { get; set; } = "Never";
    public bool IsCandidate { get; set; }
}

public class UserAddDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = null!;

    [Required]
    public int RoleId { get; set; }

    public int? CompanyId { get; set; }
    public int? TeamId { get; set; }
}

public class UserChangeRoleDto
{
    [Required]
    public int RoleId { get; set; }

    public int? CompanyId { get; set; }
    public int? TeamId { get; set; }
}

public class UserCreatedDto
{
    public int AppUserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string TemporaryPassword { get; set; } = null!;
}

// ---------- Bulk upload ----------

public class UploadRowResultDto
{
    public int Row { get; set; }
    public string Candidate { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = null!;   // Valid | Error
    public string? Message { get; set; }
}

public class UploadResultDto
{
    public int BatchId { get; set; }
    public string FileName { get; set; } = null!;
    public int Total { get; set; }
    public int Imported { get; set; }
    public int Errors { get; set; }
    public string Status { get; set; } = null!;
    public List<UploadRowResultDto> Rows { get; set; } = new();
}

public class UploadBatchDto
{
    public int BatchId { get; set; }
    public string FileName { get; set; } = null!;
    public string UploadedBy { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = null!;
    public int Total { get; set; }
    public int Imported { get; set; }
    public int Errors { get; set; }
    public string Status { get; set; } = null!;
}
