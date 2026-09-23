using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class TimesheetUploadBatchService : ITimesheetUploadBatchService
{
    private readonly ITimesheetUploadBatchRepository _repository;
    private readonly IMapper _mapper;

    public TimesheetUploadBatchService(ITimesheetUploadBatchRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TimesheetUploadBatchDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TimesheetUploadBatchDto>>(entities);
    }

    public async Task<TimesheetUploadBatchDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TimesheetUploadBatchDto>(entity);
    }

    public async Task<TimesheetUploadBatchDto> CreateAsync(TimesheetUploadBatchDto dto)
    {
        var entity = _mapper.Map<TimesheetUploadBatch>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TimesheetUploadBatchDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, TimesheetUploadBatchDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.UploadBatchId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
