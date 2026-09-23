using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for TimesheetStatusHistory.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimesheetStatusHistoriesController : ControllerBase
{
    private readonly ITimesheetStatusHistoryService _service;

    public TimesheetStatusHistoriesController(ITimesheetStatusHistoryService service)
    {
        _service = service;
    }

    /// <summary>Gets all TimesheetStatusHistories.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetStatusHistoryDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single TimesheetStatusHistory by its key.</summary>
    [HttpGet("{id}", Name = "GetTimesheetStatusHistoryById")]
    public async Task<ActionResult<TimesheetStatusHistoryDto>> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new TimesheetStatusHistory.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetStatusHistoryDto>> Create([FromBody] TimesheetStatusHistoryDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTimesheetStatusHistoryById", new { id = created.TimesheetStatusHistoryId }, created);
    }

    /// <summary>Updates an existing TimesheetStatusHistory.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] TimesheetStatusHistoryDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a TimesheetStatusHistory.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
