using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class CandidateApprovalRepository : Repository<CandidateApproval>, ICandidateApprovalRepository
{
    public CandidateApprovalRepository(TmsDbContext context) : base(context)
    {
    }
}
