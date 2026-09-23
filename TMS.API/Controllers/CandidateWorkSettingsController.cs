using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for CandidateWorkSetting.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandidateWorkSettingsController : ControllerBase
{
    private readonly ICandidateWorkSettingService _service;

    public CandidateWorkSettingsController(ICandidateWorkSettingService service)
    {
        _service = service;
    }

    /// <summary>Gets all CandidateWorkSettings.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateWorkSettingDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single CandidateWorkSetting by its key.</summary>
    [HttpGet("{id}", Name = "GetCandidateWorkSettingById")]
    public async Task<ActionResult<CandidateWorkSettingDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new CandidateWorkSetting.</summary>
    [HttpPost]
    public async Task<ActionResult<CandidateWorkSettingDto>> Create([FromBody] CandidateWorkSettingDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetCandidateWorkSettingById", new { id = created.CandidateId }, created);
    }

    /// <summary>Updates an existing CandidateWorkSetting.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CandidateWorkSettingDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a CandidateWorkSetting.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
