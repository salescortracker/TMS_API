using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;

namespace TMS.BusinessLayer.Services.Workflow;

public interface ILookupService
{
    Task<LookupsDto> GetLookupsAsync();
}

public class LookupService : ILookupService
{
    private readonly TmsDbContext _db;

    public LookupService(TmsDbContext db)
    {
        _db = db;
    }

    public async Task<LookupsDto> GetLookupsAsync() => new()
    {
        Companies = await _db.Companies.Where(c => c.IsActive).OrderBy(c => c.CompanyName)
            .Select(c => new LookupItemDto { Id = c.CompanyId, Name = c.CompanyName, Extra = c.EmailDomain })
            .ToListAsync(),
        Teams = await _db.Teams.Where(t => t.IsActive).OrderBy(t => t.TeamId)
            .Select(t => new LookupItemDto { Id = t.TeamId, Name = t.TeamName, Extra = t.DialCode })
            .ToListAsync(),
    };
}
