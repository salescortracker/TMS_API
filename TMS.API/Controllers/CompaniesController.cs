using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for Company.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompaniesController(ICompanyService service)
    {
        _service = service;
    }

    /// <summary>Gets all Companies.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single Company by its key.</summary>
    [HttpGet("{id}", Name = "GetCompanyById")]
    public async Task<ActionResult<CompanyDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new Company.</summary>
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create([FromBody] CompanyDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetCompanyById", new { id = created.CompanyId }, created);
    }

    /// <summary>Updates an existing Company.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CompanyDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a Company.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
