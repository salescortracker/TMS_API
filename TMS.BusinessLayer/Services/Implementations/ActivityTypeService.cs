using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class ActivityTypeService : IActivityTypeService
{
    private readonly IActivityTypeRepository _repository;
    private readonly IMapper _mapper;

    public ActivityTypeService(IActivityTypeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ActivityTypeDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ActivityTypeDto>>(entities);
    }

    public async Task<ActivityTypeDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<ActivityTypeDto>(entity);
    }

    public async Task<ActivityTypeDto> CreateAsync(ActivityTypeDto dto)
    {
        var entity = _mapper.Map<ActivityType>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ActivityTypeDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, ActivityTypeDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.ActivityTypeId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
