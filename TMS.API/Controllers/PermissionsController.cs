using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for Permission.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    /// <summary>Gets all Permissions.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single Permission by its key.</summary>
    [HttpGet("{id}", Name = "GetPermissionById")]
    public async Task<ActionResult<PermissionDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new Permission.</summary>
    [HttpPost]
    public async Task<ActionResult<PermissionDto>> Create([FromBody] PermissionDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetPermissionById", new { id = created.PermissionId }, created);
    }

    /// <summary>Updates an existing Permission.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PermissionDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a Permission.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
