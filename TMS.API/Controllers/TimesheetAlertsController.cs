using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for TimesheetAlert.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimesheetAlertsController : ControllerBase
{
    private readonly ITimesheetAlertService _service;

    public TimesheetAlertsController(ITimesheetAlertService service)
    {
        _service = service;
    }

    /// <summary>Gets all TimesheetAlerts.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetAlertDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single TimesheetAlert by its key.</summary>
    [HttpGet("{id}", Name = "GetTimesheetAlertById")]
    public async Task<ActionResult<TimesheetAlertDto>> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new TimesheetAlert.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetAlertDto>> Create([FromBody] TimesheetAlertDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTimesheetAlertById", new { id = created.TimesheetAlertId }, created);
    }

    /// <summary>Updates an existing TimesheetAlert.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] TimesheetAlertDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a TimesheetAlert.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
