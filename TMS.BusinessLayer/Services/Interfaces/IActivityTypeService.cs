using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IActivityTypeService
{
    Task<IEnumerable<ActivityTypeDto>> GetAllAsync();

    Task<ActivityTypeDto?> GetByIdAsync(int id);

    Task<ActivityTypeDto> CreateAsync(ActivityTypeDto dto);

    Task<bool> UpdateAsync(int id, ActivityTypeDto dto);

    Task<bool> DeleteAsync(int id);
}
