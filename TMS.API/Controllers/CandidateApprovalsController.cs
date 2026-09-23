using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for CandidateApproval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandidateApprovalsController : ControllerBase
{
    private readonly ICandidateApprovalService _service;

    public CandidateApprovalsController(ICandidateApprovalService service)
    {
        _service = service;
    }

    /// <summary>Gets all CandidateApprovals.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateApprovalDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single CandidateApproval by its key.</summary>
    [HttpGet("{id}", Name = "GetCandidateApprovalById")]
    public async Task<ActionResult<CandidateApprovalDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new CandidateApproval.</summary>
    [HttpPost]
    public async Task<ActionResult<CandidateApprovalDto>> Create([FromBody] CandidateApprovalDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetCandidateApprovalById", new { id = created.CandidateApprovalId }, created);
    }

    /// <summary>Updates an existing CandidateApproval.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CandidateApprovalDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a CandidateApproval.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
