using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TimesheetDayService : ITimesheetDayService
{
    private readonly ITimesheetDayRepository _repository;
    private readonly IMapper _mapper;

    public TimesheetDayService(ITimesheetDayRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimesheetDayDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TimesheetDayDto>>(entities);
    }

    public async Task<TimesheetDayDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TimesheetDayDto>(entity);
    }

    public async Task<TimesheetDayDto> CreateAsync(TimesheetDayDto dto)
    {
        var entity = _mapper.Map<TimesheetDay>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TimesheetDayDto>(created);
    }

    public async Task<bool> UpdateAsync(long id, TimesheetDayDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.TimesheetDayId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id) =>
        await _repository.DeleteAsync(id);
}
