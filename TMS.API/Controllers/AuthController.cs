using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;

namespace TMS.API.Controllers;

/// <summary>Login for the Angular app — issues a JWT.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _user;

    public AuthController(IAuthService authService, ICurrentUser user)
    {
        _authService = authService;
        _user = user;
    }

    /// <summary>Verifies email + password against AppUser and returns a JWT with the caller's roles/permissions/menus.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return result is null ? Unauthorized(new { message = "Invalid email or password." }) : Ok(result);
    }

    /// <summary>Lets the signed-in user set a new password (required after a temporary password).</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        await _authService.ChangePasswordAsync(_user.AppUserId, request);
        return NoContent();
    }
}
