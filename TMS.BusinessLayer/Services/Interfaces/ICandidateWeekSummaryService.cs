using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ICandidateWeekSummaryService
{
    Task<IEnumerable<CandidateWeekSummaryDto>> GetAllAsync();

    Task<IEnumerable<CandidateWeekSummaryDto>> GetByCandidateIdAsync(int candidateId);
}
