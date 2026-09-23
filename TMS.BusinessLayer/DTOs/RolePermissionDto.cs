using System.ComponentModel.DataAnnotations;

namespace TMS.BusinessLayer.DTOs;

public class RolePermissionDto
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }
}
