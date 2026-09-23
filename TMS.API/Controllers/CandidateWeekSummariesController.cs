using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Read-only endpoints for the VwCandidateWeekSummary reporting view.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandidateWeekSummariesController : ControllerBase
{
    private readonly ICandidateWeekSummaryService _service;

    public CandidateWeekSummariesController(ICandidateWeekSummaryService service)
    {
        _service = service;
    }

    /// <summary>Gets all candidate week summaries.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CandidateWeekSummaryDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets the week summaries for one candidate.</summary>
    [HttpGet("candidate/{candidateId}")]
    public async Task<ActionResult<IEnumerable<CandidateWeekSummaryDto>>> GetByCandidateId(int candidateId)
    {
        var items = await _service.GetByCandidateIdAsync(candidateId);
        return Ok(items);
    }
}
