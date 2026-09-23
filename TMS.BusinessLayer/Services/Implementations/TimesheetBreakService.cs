using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TimesheetBreakService : ITimesheetBreakService
{
    private readonly ITimesheetBreakRepository _repository;
    private readonly IMapper _mapper;

    public TimesheetBreakService(ITimesheetBreakRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimesheetBreakDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TimesheetBreakDto>>(entities);
    }

    public async Task<TimesheetBreakDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TimesheetBreakDto>(entity);
    }

    public async Task<TimesheetBreakDto> CreateAsync(TimesheetBreakDto dto)
    {
        var entity = _mapper.Map<TimesheetBreak>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TimesheetBreakDto>(created);
    }

    public async Task<bool> UpdateAsync(long id, TimesheetBreakDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.TimesheetBreakId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id) =>
        await _repository.DeleteAsync(id);
}
