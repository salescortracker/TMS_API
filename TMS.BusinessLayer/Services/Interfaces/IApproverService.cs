using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IApproverService
{
    Task<IEnumerable<ApproverDto>> GetAllAsync();

    Task<ApproverDto?> GetByIdAsync(int id);

    Task<ApproverDto> CreateAsync(ApproverDto dto);

    Task<bool> UpdateAsync(int id, ApproverDto dto);

    Task<bool> DeleteAsync(int id);
}
