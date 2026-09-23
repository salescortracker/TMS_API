using Microsoft.EntityFrameworkCore;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.DataAccessLayer.Repositories.Implementations;

public class CandidateWeekSummaryRepository : ICandidateWeekSummaryRepository
{
    private readonly TmsDbContext _context;

    public CandidateWeekSummaryRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VwCandidateWeekSummary>> GetAllAsync() =>
        await _context.VwCandidateWeekSummaries.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<VwCandidateWeekSummary>> GetByCandidateIdAsync(int candidateId) =>
        await _context.VwCandidateWeekSummaries
            .AsNoTracking()
            .Where(s => s.CandidateId == candidateId)
            .ToListAsync();
}
