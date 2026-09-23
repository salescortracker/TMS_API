using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class CandidateWorkSettingService : ICandidateWorkSettingService
{
    private readonly ICandidateWorkSettingRepository _repository;
    private readonly IMapper _mapper;

    public CandidateWorkSettingService(ICandidateWorkSettingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CandidateWorkSettingDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CandidateWorkSettingDto>>(entities);
    }

    public async Task<CandidateWorkSettingDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CandidateWorkSettingDto>(entity);
    }

    public async Task<CandidateWorkSettingDto> CreateAsync(CandidateWorkSettingDto dto)
    {
        var entity = _mapper.Map<CandidateWorkSetting>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<CandidateWorkSettingDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, CandidateWorkSettingDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.CandidateId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
