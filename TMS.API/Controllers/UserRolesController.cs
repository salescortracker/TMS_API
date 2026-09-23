using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for UserRole.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _service;

    public UserRolesController(IUserRoleService service)
    {
        _service = service;
    }

    /// <summary>Gets all UserRoles.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single UserRole by its key.</summary>
    [HttpGet("{id}", Name = "GetUserRoleById")]
    public async Task<ActionResult<UserRoleDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new UserRole.</summary>
    [HttpPost]
    public async Task<ActionResult<UserRoleDto>> Create([FromBody] UserRoleDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetUserRoleById", new { id = created.UserRoleId }, created);
    }

    /// <summary>Updates an existing UserRole.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserRoleDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a UserRole.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
