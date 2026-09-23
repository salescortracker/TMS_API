using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITimesheetStatusHistoryService
{
    Task<IEnumerable<TimesheetStatusHistoryDto>> GetAllAsync();

    Task<TimesheetStatusHistoryDto?> GetByIdAsync(long id);

    Task<TimesheetStatusHistoryDto> CreateAsync(TimesheetStatusHistoryDto dto);

    Task<bool> UpdateAsync(long id, TimesheetStatusHistoryDto dto);

    Task<bool> DeleteAsync(long id);
}
