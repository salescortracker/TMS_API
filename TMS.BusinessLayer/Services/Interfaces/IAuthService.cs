using TMS.BusinessLayer.DTOs;

namespace TMS.BusinessLayer.Services.Interfaces;

public interface IAuthService
{
    /// <summary>Returns a login response on success, or null when the email/password don't match.</summary>
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    Task ChangePasswordAsync(int appUserId, ChangePasswordDto request);
}
