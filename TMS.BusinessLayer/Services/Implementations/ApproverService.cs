using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class ApproverService : IApproverService
{
    private readonly IApproverRepository _repository;
    private readonly IMapper _mapper;

    public ApproverService(IApproverRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ApproverDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ApproverDto>>(entities);
    }

    public async Task<ApproverDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<ApproverDto>(entity);
    }

    public async Task<ApproverDto> CreateAsync(ApproverDto dto)
    {
        var entity = _mapper.Map<Approver>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ApproverDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, ApproverDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.ApproverId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
