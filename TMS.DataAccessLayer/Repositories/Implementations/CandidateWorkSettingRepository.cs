using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class CandidateWorkSettingRepository : Repository<CandidateWorkSetting>, ICandidateWorkSettingRepository
{
    public CandidateWorkSettingRepository(TmsDbContext context) : base(context)
    {
    }
}
