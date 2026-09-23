using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TimesheetAlertService : ITimesheetAlertService
{
    private readonly ITimesheetAlertRepository _repository;
    private readonly IMapper _mapper;

    public TimesheetAlertService(ITimesheetAlertRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimesheetAlertDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TimesheetAlertDto>>(entities);
    }

    public async Task<TimesheetAlertDto?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TimesheetAlertDto>(entity);
    }

    public async Task<TimesheetAlertDto> CreateAsync(TimesheetAlertDto dto)
    {
        var entity = _mapper.Map<TimesheetAlert>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TimesheetAlertDto>(created);
    }

    public async Task<bool> UpdateAsync(long id, TimesheetAlertDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.TimesheetAlertId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(long id) =>
        await _repository.DeleteAsync(id);
}
