using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for ActivityType.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ActivityTypesController : ControllerBase
{
    private readonly IActivityTypeService _service;

    public ActivityTypesController(IActivityTypeService service)
    {
        _service = service;
    }

    /// <summary>Gets all ActivityTypes.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityTypeDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single ActivityType by its key.</summary>
    [HttpGet("{id}", Name = "GetActivityTypeById")]
    public async Task<ActionResult<ActivityTypeDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new ActivityType.</summary>
    [HttpPost]
    public async Task<ActionResult<ActivityTypeDto>> Create([FromBody] ActivityTypeDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetActivityTypeById", new { id = created.ActivityTypeId }, created);
    }

    /// <summary>Updates an existing ActivityType.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActivityTypeDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a ActivityType.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
