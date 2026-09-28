using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TMS.BusinessLayer.Common;
using TMS.BusinessLayer.DTOs.Workflow;
using TMS.DataAccessLayer.Context;
using TMS.DataAccessLayer.Entities;
using TMS.Utilities.Constants;
using TMS.Utilities.Exceptions;

namespace TMS.BusinessLayer.Services.Workflow;

public interface IAccessAdminService
{
    Task<RolesOverviewDto> GetRolesAsync();

    Task<RoleSummaryDto> CreateRoleAsync(int actorUserId, RoleSaveDto dto);

    Task<RoleSummaryDto> UpdateRoleAsync(int actorUserId, int roleId, RoleSaveDto dto);

    Task DeleteRoleAsync(int actorUserId, int roleId);

    Task<List<StaffUserDto>> GetUsersAsync(string? search);

    Task<UserCreatedDto> AddUserAsync(int actorUserId, UserAddDto dto);

    Task ChangeRoleAsync(int actorUserId, int appUserId, UserChangeRoleDto dto);

    Task SetActiveAsync(int actorUserId, int appUserId, bool active);

    Task<UserCreatedDto> ResetPasswordAsync(int actorUserId, int appUserId);
}

public class AccessAdminService : IAccessAdminService
{
    private readonly TmsDbContext _db;
    private readonly IAuditService _audit;

    public AccessAdminService(TmsDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    // ---------- Roles ----------

    public async Task<RolesOverviewDto> GetRolesAsync()
    {
        var roles = await _db.Roles
            .Include(r => r.RoleMenus).ThenInclude(rm => rm.Menu)
            .Include(r => r.Permissions)
            .Include(r => r.UserRoles)
            .OrderBy(r => r.RoleId).ToListAsync();

        return new RolesOverviewDto
        {
            Roles = roles.Select(ToSummary).ToList(),
            AllMenus = await _db.Menus.OrderBy(m => m.DisplayOrder)
                .Select(m => new MenuItemDto { Code = m.MenuCode, Name = m.MenuName }).ToListAsync(),
            AllPermissions = await _db.Permissions.OrderBy(p => p.PermissionId)
                .Select(p => new PermissionItemDto { Code = p.PermissionCode, Description = p.Description ?? string.Empty }).ToListAsync(),
        };
    }

    private static RoleSummaryDto ToSummary(Role r) => new()
    {
        RoleId = r.RoleId,
        Name = r.RoleName,
        Description = r.Description ?? string.Empty,
        IsBuiltIn = r.IsBuiltIn,
        People = r.UserRoles.Count,
        Menus = r.RoleMenus.OrderBy(rm => rm.Menu.DisplayOrder).Select(rm => rm.Menu.MenuCode).ToList(),
        Permissions = r.Permissions.Select(p => p.PermissionCode).ToList(),
    };

    private async Task ApplyAccessAsync(Role role, RoleSaveDto dto)
    {
        var menus = await _db.Menus.Where(m => dto.Menus.Contains(m.MenuCode)).ToListAsync();
        var perms = await _db.Permissions.Where(p => dto.Permissions.Contains(p.PermissionCode)).ToListAsync();

        role.RoleMenus.Clear();
        foreach (var m in menus) role.RoleMenus.Add(new RoleMenu { MenuId = m.MenuId });

        role.Permissions.Clear();
        foreach (var p in perms) role.Permissions.Add(p);
    }

    private async Task<RoleSummaryDto> LoadSummaryAsync(int roleId)
    {
        var r = await _db.Roles.AsNoTracking()
            .Include(x => x.RoleMenus).ThenInclude(rm => rm.Menu)
            .Include(x => x.Permissions).Include(x => x.UserRoles)
            .FirstAsync(x => x.RoleId == roleId);
        return ToSummary(r);
    }

    public async Task<RoleSummaryDto> CreateRoleAsync(int actorUserId, RoleSaveDto dto)
    {
        var name = dto.Name.Trim();
        if (await _db.Roles.AnyAsync(r => r.RoleName == name))
            throw new BusinessRuleException($"A role named \"{name}\" already exists.");

        var role = new Role { RoleName = name, Description = dto.Description?.Trim(), IsBuiltIn = false };
        await ApplyAccessAsync(role, dto);
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        await LogAsync(actorUserId, "RoleCreated", role.RoleId, $"Created role {name}");
        return await LoadSummaryAsync(role.RoleId);
    }

    public async Task<RoleSummaryDto> UpdateRoleAsync(int actorUserId, int roleId, RoleSaveDto dto)
    {
        var role = await _db.Roles.Include(r => r.RoleMenus).Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId) ?? throw new NotFoundException("Role not found.");

        var name = dto.Name.Trim();
        if (role.IsBuiltIn && !string.Equals(role.RoleName, name, StringComparison.Ordinal))
            throw new BusinessRuleException("Built-in roles cannot be renamed.");
        if (await _db.Roles.AnyAsync(r => r.RoleName == name && r.RoleId != roleId))
            throw new BusinessRuleException($"A role named \"{name}\" already exists.");
        if (role.RoleName == RoleNames.Admin &&
            (!dto.Permissions.Contains(PermissionCodes.ManageRolesUsers) || !dto.Menus.Contains("roles_access")))
            throw new BusinessRuleException("Admin must keep Roles & Access, otherwise nobody could manage access.");

        role.RoleName = name;
        role.Description = dto.Description?.Trim();
        await ApplyAccessAsync(role, dto);
        await _db.SaveChangesAsync();

        await LogAsync(actorUserId, "RoleUpdated", role.RoleId, $"Updated access for role {name}");
        return await LoadSummaryAsync(roleId);
    }

    public async Task DeleteRoleAsync(int actorUserId, int roleId)
    {
        var role = await _db.Roles.Include(r => r.UserRoles).Include(r => r.RoleMenus).Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId) ?? throw new NotFoundException("Role not found.");

        if (role.IsBuiltIn) throw new BusinessRuleException("Built-in roles cannot be deleted.");
        if (role.UserRoles.Count > 0) throw new BusinessRuleException("Move the people in this role to another role first.");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "RoleDeleted", roleId, $"Deleted role {role.RoleName}");
    }

    // ---------- Users ----------

    public async Task<List<StaffUserDto>> GetUsersAsync(string? search)
    {
        var q = _db.AppUsers.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Company)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Team).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var t = search.Trim();
            q = q.Where(u => u.FullName.Contains(t) || u.Email.Contains(t));
        }

        var users = await q.OrderBy(u => u.FullName).ToListAsync();
        return users.Select(u =>
        {
            var ur = u.UserRoles.FirstOrDefault();
            var scope = ur is null ? "—" :
                ur.CompanyId is null && ur.TeamId is null ? "All companies & teams" :
                string.Join(" · ", new[] { ur.Company?.CompanyName, ur.Team?.TeamName }.Where(s => !string.IsNullOrEmpty(s)));
            return new StaffUserDto
            {
                AppUserId = u.AppUserId,
                Name = u.FullName,
                Email = u.Email,
                Role = ur?.Role.RoleName ?? "—",
                Scope = scope,
                Status = u.IsActive ? "Active" : "Inactive",
                LastLogin = u.LastLoginAt is null ? "Never" :
                    u.LastLoginAt.Value.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
                IsCandidate = ur?.Role.RoleName == RoleNames.Candidate,
            };
        }).ToList();
    }

    public async Task<UserCreatedDto> AddUserAsync(int actorUserId, UserAddDto dto)
    {
        var email = dto.Email.Trim();
        if (await _db.AppUsers.AnyAsync(u => u.Email == email))
            throw new BusinessRuleException("A user with this email already exists.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId)
            ?? throw new NotFoundException("Role not found.");
        if (role.RoleName == RoleNames.Candidate)
            throw new BusinessRuleException("Candidates are created through Onboarding, not here.");

        var temp = PasswordHasher.GenerateTemporary();
        var user = new AppUser
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(temp),
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        user.UserRoles.Add(new UserRole
        {
            RoleId = role.RoleId, CompanyId = dto.CompanyId, TeamId = dto.TeamId, GrantedAt = DateTime.UtcNow,
        });
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();

        await LogAsync(actorUserId, "UserCreated", user.AppUserId, $"Added {user.FullName} as {role.RoleName}");
        return new UserCreatedDto { AppUserId = user.AppUserId, Name = user.FullName, Email = user.Email, TemporaryPassword = temp };
    }

    private async Task<AppUser> FindUserAsync(int appUserId) =>
        await _db.AppUsers.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.AppUserId == appUserId) ?? throw new NotFoundException("User not found.");

    public async Task ChangeRoleAsync(int actorUserId, int appUserId, UserChangeRoleDto dto)
    {
        var user = await FindUserAsync(appUserId);
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId)
            ?? throw new NotFoundException("Role not found.");
        var current = user.UserRoles.FirstOrDefault();

        if (current?.Role.RoleName == RoleNames.Candidate || role.RoleName == RoleNames.Candidate)
            throw new BusinessRuleException("Candidate accounts keep the Candidate role.");
        if (appUserId == actorUserId && role.RoleName != RoleNames.Admin && current?.Role.RoleName == RoleNames.Admin)
            throw new BusinessRuleException("You cannot remove your own Admin access.");

        // A role change keeps the person's existing company/team scope unless a new one is given.
        var companyId = dto.CompanyId ?? current?.CompanyId;
        var teamId = dto.TeamId ?? current?.TeamId;
        _db.UserRoles.RemoveRange(user.UserRoles);
        _db.UserRoles.Add(new UserRole
        {
            AppUserId = appUserId, RoleId = role.RoleId, CompanyId = companyId, TeamId = teamId,
            GrantedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "RoleChanged", appUserId, $"Changed {user.FullName} to {role.RoleName}");
    }

    public async Task SetActiveAsync(int actorUserId, int appUserId, bool active)
    {
        var user = await FindUserAsync(appUserId);
        if (!active && appUserId == actorUserId)
            throw new BusinessRuleException("You cannot deactivate your own account.");

        user.IsActive = active;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, active ? "UserActivated" : "UserDeactivated", appUserId,
            $"{(active ? "Activated" : "Deactivated")} {user.FullName}");
    }

    public async Task<UserCreatedDto> ResetPasswordAsync(int actorUserId, int appUserId)
    {
        var user = await FindUserAsync(appUserId);
        var temp = PasswordHasher.GenerateTemporary();
        user.PasswordHash = PasswordHasher.Hash(temp);
        user.MustChangePassword = true;
        await _db.SaveChangesAsync();
        await LogAsync(actorUserId, "PasswordReset", appUserId, $"Reset the password for {user.FullName}");
        return new UserCreatedDto { AppUserId = user.AppUserId, Name = user.FullName, Email = user.Email, TemporaryPassword = temp };
    }

    private async Task LogAsync(int actorUserId, string action, long entityId, string description)
    {
        var actor = await _db.AppUsers.Where(u => u.AppUserId == actorUserId).Select(u => u.FullName).FirstOrDefaultAsync();
        await _audit.LogAsync(new AuditEntry(actorUserId, LogCategory.EditsRoles, Severity.Info, action,
            "Access", entityId, null, $"{actor} {char.ToLowerInvariant(description[0])}{description[1..]}"));
    }
}
