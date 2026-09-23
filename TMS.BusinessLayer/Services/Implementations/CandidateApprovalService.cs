using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class CandidateApprovalService : ICandidateApprovalService
{
    private readonly ICandidateApprovalRepository _repository;
    private readonly IMapper _mapper;

    public CandidateApprovalService(ICandidateApprovalRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CandidateApprovalDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CandidateApprovalDto>>(entities);
    }

    public async Task<CandidateApprovalDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CandidateApprovalDto>(entity);
    }

    public async Task<CandidateApprovalDto> CreateAsync(CandidateApprovalDto dto)
    {
        var entity = _mapper.Map<CandidateApproval>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<CandidateApprovalDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, CandidateApprovalDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.CandidateApprovalId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
