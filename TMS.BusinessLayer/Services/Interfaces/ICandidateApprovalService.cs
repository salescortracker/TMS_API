using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ICandidateApprovalService
{
    Task<IEnumerable<CandidateApprovalDto>> GetAllAsync();

    Task<CandidateApprovalDto?> GetByIdAsync(int id);

    Task<CandidateApprovalDto> CreateAsync(CandidateApprovalDto dto);

    Task<bool> UpdateAsync(int id, CandidateApprovalDto dto);

    Task<bool> DeleteAsync(int id);
}
