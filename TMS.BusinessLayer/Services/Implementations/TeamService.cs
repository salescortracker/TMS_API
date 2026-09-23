using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;
    private readonly IMapper _mapper;

    public TeamService(ITeamRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TeamDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TeamDto>>(entities);
    }

    public async Task<TeamDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TeamDto>(entity);
    }

    public async Task<TeamDto> CreateAsync(TeamDto dto)
    {
        var entity = _mapper.Map<Team>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TeamDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, TeamDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.TeamId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
