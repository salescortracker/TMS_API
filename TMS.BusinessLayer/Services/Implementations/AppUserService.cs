using AutoMapper;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Entities;
using TMS.DataAccessLayer.Repositories.Interfaces;

namespace TMS.BusinessLayer.Services.Implementations;

public class AppUserService : IAppUserService
{
    private readonly IAppUserRepository _repository;
    private readonly IMapper _mapper;

    public AppUserService(IAppUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AppUserDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<AppUserDto>>(entities);
    }

    public async Task<AppUserDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<AppUserDto>(entity);
    }

    public async Task<AppUserDto> CreateAsync(AppUserDto dto)
    {
        var entity = _mapper.Map<AppUser>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<AppUserDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, AppUserDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        _mapper.Map(dto, existing);
        existing.AppUserId = id;
        await _repository.UpdateAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);
}
