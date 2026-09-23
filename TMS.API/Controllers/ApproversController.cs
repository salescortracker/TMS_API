using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for Approver.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApproversController : ControllerBase
{
    private readonly IApproverService _service;

    public ApproversController(IApproverService service)
    {
        _service = service;
    }

    /// <summary>Gets all Approvers.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApproverDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single Approver by its key.</summary>
    [HttpGet("{id}", Name = "GetApproverById")]
    public async Task<ActionResult<ApproverDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new Approver.</summary>
    [HttpPost]
    public async Task<ActionResult<ApproverDto>> Create([FromBody] ApproverDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetApproverById", new { id = created.ApproverId }, created);
    }

    /// <summary>Updates an existing Approver.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApproverDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a Approver.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
