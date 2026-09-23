using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ICandidateOnboardingService
{
    Task<IEnumerable<CandidateOnboardingDto>> GetAllAsync();

    Task<CandidateOnboardingDto?> GetByIdAsync(int id);

    Task<CandidateOnboardingDto> CreateAsync(CandidateOnboardingDto dto);

    Task<bool> UpdateAsync(int id, CandidateOnboardingDto dto);

    Task<bool> DeleteAsync(int id);
}
