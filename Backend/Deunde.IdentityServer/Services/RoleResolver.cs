using System.Security.Claims;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public class RoleResolver(UserManager<IdentityUser> userManager, ILogger<RoleResolver> logger) : IRoleResolver
{
    public async Task<Claim> ResolveUserRoleClaimAsync(IdentityUser user, IEnumerable<string>? acrValues)
    {
        var requestedRole = acrValues?
            .FirstOrDefault(v => v.StartsWith("role:", StringComparison.OrdinalIgnoreCase))
            ?.Split(':').LastOrDefault()
            ?.Trim();
        
        logger.LogWarning($"Requested role {requestedRole}");

        var userRoles = await userManager.GetRolesAsync(user);

        string roleToAssign;

        if (!string.IsNullOrEmpty(requestedRole))
        {
            var matchedRole = userRoles.FirstOrDefault(r => r.Equals(requestedRole, StringComparison.OrdinalIgnoreCase));

            roleToAssign = matchedRole ?? userRoles.FirstOrDefault() ?? UserCreateManager.ClientRole;
        }
        else
        {
            roleToAssign = userRoles.FirstOrDefault() ?? UserCreateManager.ClientRole;
        }

        return new Claim(JwtClaimTypes.Role, roleToAssign);
    }
}