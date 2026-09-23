using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IActivityLogService
{
    Task<IEnumerable<ActivityLogDto>> GetAllAsync();

    Task<ActivityLogDto?> GetByIdAsync(long id);

    Task<ActivityLogDto> CreateAsync(ActivityLogDto dto);

    Task<bool> UpdateAsync(long id, ActivityLogDto dto);

    Task<bool> DeleteAsync(long id);
}
