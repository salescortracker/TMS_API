using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITimesheetBreakService
{
    Task<IEnumerable<TimesheetBreakDto>> GetAllAsync();

    Task<TimesheetBreakDto?> GetByIdAsync(long id);

    Task<TimesheetBreakDto> CreateAsync(TimesheetBreakDto dto);

    Task<bool> UpdateAsync(long id, TimesheetBreakDto dto);

    Task<bool> DeleteAsync(long id);
}
