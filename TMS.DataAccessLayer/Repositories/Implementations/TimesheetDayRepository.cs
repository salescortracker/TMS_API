using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TimesheetDayRepository : Repository<TimesheetDay>, ITimesheetDayRepository
{
    public TimesheetDayRepository(TmsDbContext context) : base(context)
    {
    }
}
