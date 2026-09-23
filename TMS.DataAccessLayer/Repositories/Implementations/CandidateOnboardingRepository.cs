using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class CandidateOnboardingRepository : Repository<CandidateOnboarding>, ICandidateOnboardingRepository
{
    public CandidateOnboardingRepository(TmsDbContext context) : base(context)
    {
    }
}
