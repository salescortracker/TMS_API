using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class UserRoleDto
{
    public int UserRoleId { get; set; }

    public int AppUserId { get; set; }

    public int RoleId { get; set; }

    public int? CompanyId { get; set; }

    public int? TeamId { get; set; }

    public DateTime GrantedAt { get; set; }
}
