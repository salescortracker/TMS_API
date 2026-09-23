using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class ActivityTypeRepository : Repository<ActivityType>, IActivityTypeRepository
{
    public ActivityTypeRepository(TmsDbContext context) : base(context)
    {
    }
}
