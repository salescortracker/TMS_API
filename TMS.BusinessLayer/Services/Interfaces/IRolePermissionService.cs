using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IRolePermissionService
{
    Task<IEnumerable<RolePermissionDto>> GetAllAsync();

    Task<RolePermissionDto?> GetByIdAsync(int roleId, int permissionId);

    Task<RolePermissionDto> CreateAsync(RolePermissionDto dto);

    Task<bool> UpdateAsync(int roleId, int permissionId, RolePermissionDto dto);

    Task<bool> DeleteAsync(int roleId, int permissionId);
}
