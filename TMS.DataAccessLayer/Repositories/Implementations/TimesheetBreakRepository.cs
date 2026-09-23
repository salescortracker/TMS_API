using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TimesheetBreakRepository : Repository<TimesheetBreak>, ITimesheetBreakRepository
{
    public TimesheetBreakRepository(TmsDbContext context) : base(context)
    {
    }
}
