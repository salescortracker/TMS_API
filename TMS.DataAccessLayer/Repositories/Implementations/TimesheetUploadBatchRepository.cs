using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TimesheetUploadBatchRepository : Repository<TimesheetUploadBatch>, ITimesheetUploadBatchRepository
{
    public TimesheetUploadBatchRepository(TmsDbContext context) : base(context)
    {
    }
}
