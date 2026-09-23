using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class ApproverRepository : Repository<Approver>, IApproverRepository
{
    public ApproverRepository(TmsDbContext context) : base(context)
    {
    }
}
