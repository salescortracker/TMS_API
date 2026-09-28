namespace TMS.BusinessLayer.Common;

/// <summary>The signed-in caller, read from the JWT. Implemented in the API project.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    int AppUserId { get; }

    string FullName { get; }

    string Email { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);

    bool HasPermission(string permissionCode);

    bool HasMenu(string menuCode);
}
