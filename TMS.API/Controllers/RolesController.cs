using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for Role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    /// <summary>Gets all Roles.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single Role by its key.</summary>
    [HttpGet("{id}", Name = "GetRoleById")]
    public async Task<ActionResult<RoleDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new Role.</summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] RoleDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetRoleById", new { id = created.RoleId }, created);
    }

    /// <summary>Updates an existing Role.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RoleDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a Role.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
