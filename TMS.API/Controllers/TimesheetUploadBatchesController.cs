using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for TimesheetUploadBatch.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TimesheetUploadBatchesController : ControllerBase
{
    private readonly ITimesheetUploadBatchService _service;

    public TimesheetUploadBatchesController(ITimesheetUploadBatchService service)
    {
        _service = service;
    }

    /// <summary>Gets all TimesheetUploadBatches.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TimesheetUploadBatchDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single TimesheetUploadBatch by its key.</summary>
    [HttpGet("{id}", Name = "GetTimesheetUploadBatchById")]
    public async Task<ActionResult<TimesheetUploadBatchDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new TimesheetUploadBatch.</summary>
    [HttpPost]
    public async Task<ActionResult<TimesheetUploadBatchDto>> Create([FromBody] TimesheetUploadBatchDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetTimesheetUploadBatchById", new { id = created.UploadBatchId }, created);
    }

    /// <summary>Updates an existing TimesheetUploadBatch.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TimesheetUploadBatchDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a TimesheetUploadBatch.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
