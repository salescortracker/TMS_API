using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for ActivityLog.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ActivityLogsController : ControllerBase
{
    private readonly IActivityLogService _service;

    public ActivityLogsController(IActivityLogService service)
    {
        _service = service;
    }

    /// <summary>Gets all ActivityLogs.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single ActivityLog by its key.</summary>
    [HttpGet("{id}", Name = "GetActivityLogById")]
    public async Task<ActionResult<ActivityLogDto>> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new ActivityLog.</summary>
    [HttpPost]
    public async Task<ActionResult<ActivityLogDto>> Create([FromBody] ActivityLogDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetActivityLogById", new { id = created.ActivityLogId }, created);
    }

    /// <summary>Updates an existing ActivityLog.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] ActivityLogDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a ActivityLog.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
