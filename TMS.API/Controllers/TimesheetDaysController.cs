using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for TimesheetDay.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimesheetDaysController : ControllerBase
{
    private readonly ITimesheetDayService _service;

    public TimesheetDaysController(ITimesheetDayService service)
    {
        _service = service;
    }

    /// <summary>Gets all TimesheetDays.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetDayDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single TimesheetDay by its key.</summary>
    [HttpGet("{id}", Name = "GetTimesheetDayById")]
    public async Task<ActionResult<TimesheetDayDto>> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new TimesheetDay.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetDayDto>> Create([FromBody] TimesheetDayDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTimesheetDayById", new { id = created.TimesheetDayId }, created);
    }

    /// <summary>Updates an existing TimesheetDay.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] TimesheetDayDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a TimesheetDay.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
