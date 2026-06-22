using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;

namespace MicroserviceApiKernel;

public class RoleRequirement : IAuthorizationRequirement
{
    public RoleRequirement(string role)
    {
        Role = role;
    }

    public string Role { get; }
}

public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        var roleClaims = context.User.Claims.Where(c =>
            c.Type == "role" ||
            c.Type == System.Security.Claims.ClaimTypes.Role);

        var hasMatchingRole = roleClaims.Any(c =>
            string.Equals(c.Value, requirement.Role, StringComparison.OrdinalIgnoreCase));

        if (hasMatchingRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
