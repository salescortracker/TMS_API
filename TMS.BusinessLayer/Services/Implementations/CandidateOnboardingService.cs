using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class CandidateOnboardingService : ICandidateOnboardingService
{
    private readonly ICandidateOnboardingRepository _repository;
    private readonly IMapper _mapper;

    public CandidateOnboardingService(ICandidateOnboardingRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CandidateOnboardingDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CandidateOnboardingDto>>(entities);
    }

    public async Task<CandidateOnboardingDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CandidateOnboardingDto>(entity);
    }

    public async Task<CandidateOnboardingDto> CreateAsync(CandidateOnboardingDto dto)
    {
        var entity = _mapper.Map<CandidateOnboarding>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<CandidateOnboardingDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, CandidateOnboardingDto dto)
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
