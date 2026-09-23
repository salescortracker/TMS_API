using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITimesheetAlertService
{
    Task<IEnumerable<TimesheetAlertDto>> GetAllAsync();

    Task<TimesheetAlertDto?> GetByIdAsync(long id);

    Task<TimesheetAlertDto> CreateAsync(TimesheetAlertDto dto);

    Task<bool> UpdateAsync(long id, TimesheetAlertDto dto);

    Task<bool> DeleteAsync(long id);
}
