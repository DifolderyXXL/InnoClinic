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
        if (context.User.Claims.Any(c => c is { Type: "role" or JwtClaimTypes.Role } && c.Value == requirement.Role))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
