using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITimesheetUploadBatchService
{
    Task<IEnumerable<TimesheetUploadBatchDto>> GetAllAsync();

    Task<TimesheetUploadBatchDto?> GetByIdAsync(int id);

    Task<TimesheetUploadBatchDto> CreateAsync(TimesheetUploadBatchDto dto);

    Task<bool> UpdateAsync(int id, TimesheetUploadBatchDto dto);

    Task<bool> DeleteAsync(int id);
}
