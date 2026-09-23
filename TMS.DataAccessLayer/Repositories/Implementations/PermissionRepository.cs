using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(TmsDbContext context) : base(context)
    {
    }
}
