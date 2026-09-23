using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for TimesheetBreak.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimesheetBreaksController : ControllerBase
{
    private readonly ITimesheetBreakService _service;

    public TimesheetBreaksController(ITimesheetBreakService service)
    {
        _service = service;
    }

    /// <summary>Gets all TimesheetBreaks.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetBreakDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single TimesheetBreak by its key.</summary>
    [HttpGet("{id}", Name = "GetTimesheetBreakById")]
    public async Task<ActionResult<TimesheetBreakDto>> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new TimesheetBreak.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetBreakDto>> Create([FromBody] TimesheetBreakDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTimesheetBreakById", new { id = created.TimesheetBreakId }, created);
    }

    /// <summary>Updates an existing TimesheetBreak.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] TimesheetBreakDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a TimesheetBreak.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
