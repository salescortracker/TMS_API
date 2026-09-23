using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TimesheetStatusHistoryService : ITimesheetStatusHistoryService
{
    private readonly ITimesheetStatusHistoryRepository _repository;
    private readonly IMapper _mapper;

    public TimesheetStatusHistoryService(ITimesheetStatusHistoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimesheetStatusHistoryDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TimesheetStatusHistoryDto>>(entities);
    }

    public async Task<TimesheetStatusHistoryDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TimesheetStatusHistoryDto>(entity);
    }

    public async Task<TimesheetStatusHistoryDto> CreateAsync(TimesheetStatusHistoryDto dto)
    {
        var entity = _mapper.Map<TimesheetStatusHistory>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TimesheetStatusHistoryDto>(created);
    }

    public async Task<bool> UpdateAsync(long id, TimesheetStatusHistoryDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.TimesheetStatusHistoryId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id) =>
        await _repository.DeleteAsync(id);
}
