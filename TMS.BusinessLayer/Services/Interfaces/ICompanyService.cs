using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();

    Task<CompanyDto?> GetByIdAsync(int id);

    Task<CompanyDto> CreateAsync(CompanyDto dto);

    Task<bool> UpdateAsync(int id, CompanyDto dto);

    Task<bool> DeleteAsync(int id);
}
