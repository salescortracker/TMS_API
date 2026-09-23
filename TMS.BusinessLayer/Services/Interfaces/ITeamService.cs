using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ITeamService
{
    Task<IEnumerable<TeamDto>> GetAllAsync();

    Task<TeamDto?> GetByIdAsync(int id);

    Task<TeamDto> CreateAsync(TeamDto dto);

    Task<bool> UpdateAsync(int id, TeamDto dto);

    Task<bool> DeleteAsync(int id);
}
