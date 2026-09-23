using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TimesheetAlertRepository : Repository<TimesheetAlert>, ITimesheetAlertRepository
{
    public TimesheetAlertRepository(TmsDbContext context) : base(context)
    {
    }
}
