using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class RolePermissionService : IRolePermissionService
{
    private readonly IRolePermissionRepository _repository;
    private readonly IMapper _mapper;

    public RolePermissionService(IRolePermissionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RolePermissionDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<RolePermissionDto>>(entities);
    }

    public async Task<RolePermissionDto?> GetByIdAsync(int roleId, int permissionId)
    {
        var entity = await _repository.GetByIdAsync(roleId, permissionId);
        return entity is null ? null : _mapper.Map<RolePermissionDto>(entity);
    }

    public async Task<RolePermissionDto> CreateAsync(RolePermissionDto dto)
    {
        var entity = _mapper.Map<RolePermission>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<RolePermissionDto>(created);
    }

    public async Task<bool> UpdateAsync(int roleId, int permissionId, RolePermissionDto dto)
    {
        var existing = await _repository.GetByIdAsync(roleId, permissionId);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.RoleId = roleId;
        existing.PermissionId = permissionId;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int roleId, int permissionId) =>
        await _repository.DeleteAsync(roleId, permissionId);
}
