using TMS.Utilities.Constants;
using Microsoft.AspNetCore.Authorization;

namespace TMS.API.Extensions;

public static class AuthorizationExtensions
{
    /// <summary>Policies "perm:{code}" and "menu:{code}" check the claims inside the JWT.</summary>
    public static IServiceCollection AddTmsAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            foreach (var code in PermissionCodes.All)
            {
                options.AddPolicy($"perm:{code}", p => p.RequireClaim("permission", code));
            }

            foreach (var code in MenuCodes.All)
            {
                options.AddPolicy($"menu:{code}", p => p.RequireClaim("menu", code));
            }

            options.AddPolicy("Candidate", p => p.RequireRole(RoleNames.Candidate));
        });

        return services;
    }
}
