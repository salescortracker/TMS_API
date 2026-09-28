using System.Security.Claims;
using TMS.BusinessLayer.Common;

namespace TMS.API.Services;

/// <summary>Reads the signed-in caller from the JWT on the current HTTP request.</summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int AppUserId
    {
        get
        {
            var raw = Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
            return int.TryParse(raw, out var id) ? id : 0;
        }
    }

    public string FullName => Principal?.FindFirstValue(ClaimTypes.Name) ?? Principal?.FindFirstValue("name") ?? string.Empty;

    public string Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email") ?? string.Empty;

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Concat(Principal.FindAll("role")).Select(c => c.Value).Distinct().ToList()
        ?? new List<string>();

    public bool IsInRole(string role) => Roles.Contains(role);

    public bool HasPermission(string permissionCode) =>
        Principal?.FindAll("permission").Any(c => c.Value == permissionCode) == true;

    public bool HasMenu(string menuCode) =>
        Principal?.FindAll("menu").Any(c => c.Value == menuCode) == true;
}
