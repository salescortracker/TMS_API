using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for Team.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _service;

    public TeamsController(ITeamService service)
    {
        _service = service;
    }

    /// <summary>Gets all Teams.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single Team by its key.</summary>
    [HttpGet("{id}", Name = "GetTeamById")]
    public async Task<ActionResult<TeamDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new Team.</summary>
    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create([FromBody] TeamDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTeamById", new { id = created.TeamId }, created);
    }

    /// <summary>Updates an existing Team.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TeamDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a Team.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
