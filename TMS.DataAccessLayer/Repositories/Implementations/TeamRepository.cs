using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(TmsDbContext context) : base(context)
    {
    }
}
