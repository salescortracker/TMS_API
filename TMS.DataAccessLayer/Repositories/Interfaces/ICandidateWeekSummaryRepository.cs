using TMS.DataAccessLayer.Entities;

namespace TMS.DataAccessLayer.Repositories.Interfaces;

/// <summary>
/// Read-only access to the VwCandidateWeekSummary view (no primary key, not mutable).
/// </summary>
public interface ICandidateWeekSummaryRepository
{
    Task<IEnumerable<VwCandidateWeekSummary>> GetAllAsync();

    Task<IEnumerable<VwCandidateWeekSummary>> GetByCandidateIdAsync(int candidateId);
}
