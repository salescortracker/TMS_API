using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int AppUserId { get; set; }

    public int RoleId { get; set; }

    public int? CompanyId { get; set; }

    public int? TeamId { get; set; }

    public DateTime GrantedAt { get; set; }

    public virtual AppUser AppUser { get; set; } = null!;

    public virtual Company? Company { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Team? Team { get; set; }
}
