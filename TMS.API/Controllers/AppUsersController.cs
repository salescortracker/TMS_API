using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>
/// Full CRUD endpoints for AppUser.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppUsersController : ControllerBase
{
    private readonly IAppUserService _service;

    public AppUsersController(IAppUserService service)
    {
        _service = service;
    }

    /// <summary>Gets all AppUsers.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUserDto>>> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>Gets a single AppUser by its key.</summary>
    [HttpGet("{id}", Name = "GetAppUserById")]
    public async Task<ActionResult<AppUserDto>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Creates a new AppUser.</summary>
    [HttpPost]
    public async Task<ActionResult<AppUserDto>> Create([FromBody] AppUserDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtRoute("GetAppUserById", new { id = created.AppUserId }, created);
    }

    /// <summary>Updates an existing AppUser.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AppUserDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    /// <summary>Deletes a AppUser.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
