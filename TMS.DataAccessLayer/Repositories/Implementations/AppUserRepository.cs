using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class AppUserRepository : Repository<AppUser>, IAppUserRepository
{
    public AppUserRepository(TmsDbContext context) : base(context)
    {
    }
}
