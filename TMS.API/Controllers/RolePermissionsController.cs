using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for RolePermission.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionService _service;

    public RolePermissionsController(IRolePermissionService service)
    {
        _service = service;
    }

    /// <summary>Gets all RolePermissions.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolePermissionDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single RolePermission by its key.</summary>
    [HttpGet("{roleId}/{permissionId}", Name = "GetRolePermissionById")]
    public async Task<ActionResult<RolePermissionDto>> GetById(int roleId, int permissionId)
    {
        var item = await _service.GetByIdAsync(roleId, permissionId);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new RolePermission.</summary>
    [HttpPost]
    public async Task<ActionResult<RolePermissionDto>> Create([FromBody] RolePermissionDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetRolePermissionById", new { roleId = created.RoleId, permissionId = created.PermissionId }, created);
    }

    /// <summary>Updates an existing RolePermission.</summary>
    [HttpPut("{roleId}/{permissionId}")]
    public async Task<IActionResult> Update(int roleId, int permissionId, [FromBody] RolePermissionDto dto)
    {
        var updated = await _service.UpdateAsync(roleId, permissionId, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a RolePermission.</summary>
    [HttpDelete("{roleId}/{permissionId}")]
    public async Task<IActionResult> Delete(int roleId, int permissionId)
    {
        var deleted = await _service.DeleteAsync(roleId, permissionId);
        return deleted ? NoContent() : NotFound();
    }
}
