using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TimesheetStatusHistoryRepository : Repository<TimesheetStatusHistory>, ITimesheetStatusHistoryRepository
{
    public TimesheetStatusHistoryRepository(TmsDbContext context) : base(context)
    {
    }
}
