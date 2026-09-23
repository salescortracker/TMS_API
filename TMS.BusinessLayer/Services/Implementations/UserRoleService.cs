using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _repository;
    private readonly IMapper _mapper;

    public UserRoleService(IUserRoleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserRoleDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserRoleDto>>(entities);
    }

    public async Task<UserRoleDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<UserRoleDto>(entity);
    }

    public async Task<UserRoleDto> CreateAsync(UserRoleDto dto)
    {
        var entity = _mapper.Map<UserRole>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<UserRoleDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, UserRoleDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.UserRoleId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
