using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs;
using TMS.BusinessLayer.Services.Interfaces;
using TMS.DataAccessLayer.Context;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly TmsDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(TmsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _context.AppUsers
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.Permissions)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RoleMenus).ThenInclude(rm => rm.Menu)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.IsActive);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var roles = user.UserRoles.Select(ur => ur.Role.RoleName).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.Permissions.Select(p => p.PermissionCode))
            .Distinct().ToList();
        var menus = user.UserRoles
            .SelectMany(ur => ur.Role.RoleMenus.Select(rm => rm.Menu))
            .OrderBy(m => m.DisplayOrder)
            .Select(m => m.MenuCode)
            .Distinct().ToList();

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var (token, expiresAt) = CreateToken(user.AppUserId, user.FullName, user.Email, roles, permissions, menus);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            AppUserId = user.AppUserId,
            FullName = user.FullName,
            Email = user.Email,
            MustChangePassword = user.MustChangePassword,
            Roles = roles,
            Permissions = permissions,
            Menus = menus,
        };
    }

    public async Task ChangePasswordAsync(int appUserId, ChangePasswordDto request)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.AppUserId == appUserId)
                   ?? throw new NotFoundException("User not found.");

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new BusinessRuleException("Your current password is incorrect.");
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        await _context.SaveChangesAsync();
    }

    private (string Token, DateTime ExpiresAt) CreateToken(
        int appUserId, string fullName, string email,
        List<string> roles, List<string> permissions, List<string> menus)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "480");
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, appUserId.ToString()),
            new(ClaimTypes.NameIdentifier, appUserId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));
        claims.AddRange(menus.Select(m => new Claim("menu", m)));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
