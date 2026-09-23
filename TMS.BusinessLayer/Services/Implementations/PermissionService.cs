using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IMapper _mapper;

    public PermissionService(IPermissionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<PermissionDto>>(entities);
    }

    public async Task<PermissionDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<PermissionDto>(entity);
    }

    public async Task<PermissionDto> CreateAsync(PermissionDto dto)
    {
        var entity = _mapper.Map<Permission>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<PermissionDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, PermissionDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.PermissionId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
