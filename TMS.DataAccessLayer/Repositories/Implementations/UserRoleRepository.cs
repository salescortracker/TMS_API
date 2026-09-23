using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(TmsDbContext context) : base(context)
    {
    }
}
