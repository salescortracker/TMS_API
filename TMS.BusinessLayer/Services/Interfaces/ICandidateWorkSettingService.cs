using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ICandidateWorkSettingService
{
    Task<IEnumerable<CandidateWorkSettingDto>> GetAllAsync();

    Task<CandidateWorkSettingDto?> GetByIdAsync(int id);

    Task<CandidateWorkSettingDto> CreateAsync(CandidateWorkSettingDto dto);

    Task<bool> UpdateAsync(int id, CandidateWorkSettingDto dto);

    Task<bool> DeleteAsync(int id);
}
