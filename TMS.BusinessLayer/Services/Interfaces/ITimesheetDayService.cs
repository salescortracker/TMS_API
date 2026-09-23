using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITimesheetDayService
{
    Task<IEnumerable<TimesheetDayDto>> GetAllAsync();

    Task<TimesheetDayDto?> GetByIdAsync(long id);

    Task<TimesheetDayDto> CreateAsync(TimesheetDayDto dto);

    Task<bool> UpdateAsync(long id, TimesheetDayDto dto);

    Task<bool> DeleteAsync(long id);
}
