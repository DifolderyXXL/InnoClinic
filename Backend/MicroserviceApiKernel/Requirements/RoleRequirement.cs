using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;

namespace MicroserviceApiKernel;

public record RoleRequirement(string[] AllowedRoles, string[] AllowedClaimTypes) : IAuthorizationRequirement;

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var userClaims = context.User.Claims
            .Where(c => requirement.AllowedClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
            .Select(c => c.Value);

        if (requirement.AllowedRoles.Any(role => userClaims.Contains(role, StringComparer .OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
