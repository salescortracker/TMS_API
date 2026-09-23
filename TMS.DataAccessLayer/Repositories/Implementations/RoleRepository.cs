using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(TmsDbContext context) : base(context)
    {
    }
}
