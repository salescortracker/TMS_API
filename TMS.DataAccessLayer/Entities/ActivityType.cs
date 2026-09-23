using System;
using System.Collections.Generic;

namespace TMS.DataAccessLayer.Entities;

public partial class ActivityType
{
    public int ActivityTypeId { get; set; }

    public string ActivityName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<TimesheetDay> TimesheetDays { get; set; } = new List<TimesheetDay>();
}
