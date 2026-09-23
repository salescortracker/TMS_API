using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for CandidateOnboarding.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandidateOnboardingsController : ControllerBase
{
    private readonly ICandidateOnboardingService _service;

    public CandidateOnboardingsController(ICandidateOnboardingService service)
    {
        _service = service;
    }

    /// <summary>Gets all CandidateOnboardings.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateOnboardingDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single CandidateOnboarding by its key.</summary>
    [HttpGet("{id}", Name = "GetCandidateOnboardingById")]
    public async Task<ActionResult<CandidateOnboardingDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new CandidateOnboarding.</summary>
    [HttpPost]
    public async Task<ActionResult<CandidateOnboardingDto>> Create([FromBody] CandidateOnboardingDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetCandidateOnboardingById", new { id = created.CandidateId }, created);
    }

    /// <summary>Updates an existing CandidateOnboarding.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CandidateOnboardingDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a CandidateOnboarding.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
