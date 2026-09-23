using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync();

    Task<PermissionDto?> GetByIdAsync(int id);

    Task<PermissionDto> CreateAsync(PermissionDto dto);

    Task<bool> UpdateAsync(int id, PermissionDto dto);

    Task<bool> DeleteAsync(int id);
}
