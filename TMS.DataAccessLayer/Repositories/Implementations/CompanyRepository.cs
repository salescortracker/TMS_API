using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class CompanyRepository : Repository<Company>, ICompanyRepository
{
    public CompanyRepository(TmsDbContext context) : base(context)
    {
    }
}
