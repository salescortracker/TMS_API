using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IAppUserService
{
    Task<IEnumerable<AppUserDto>> GetAllAsync();

    Task<AppUserDto?> GetByIdAsync(int id);

    Task<AppUserDto> CreateAsync(AppUserDto dto);

    Task<bool> UpdateAsync(int id, AppUserDto dto);

    Task<bool> DeleteAsync(int id);
}
