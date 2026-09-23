using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repository;
    private readonly IMapper _mapper;

    public ActivityLogService(IActivityLogRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ActivityLogDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ActivityLogDto>>(entities);
    }

    public async Task<ActivityLogDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<ActivityLogDto>(entity);
    }

    public async Task<ActivityLogDto> CreateAsync(ActivityLogDto dto)
    {
        var entity = _mapper.Map<ActivityLog>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ActivityLogDto>(created);
    }

    public async Task<bool> UpdateAsync(long id, ActivityLogDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.ActivityLogId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id) =>
        await _repository.DeleteAsync(id);
}
