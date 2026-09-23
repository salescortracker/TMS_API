using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IUserRoleService
{
    Task<IEnumerable<UserRoleDto>> GetAllAsync();

    Task<UserRoleDto?> GetByIdAsync(int id);

    Task<UserRoleDto> CreateAsync(UserRoleDto dto);

    Task<bool> UpdateAsync(int id, UserRoleDto dto);

    Task<bool> DeleteAsync(int id);
}
