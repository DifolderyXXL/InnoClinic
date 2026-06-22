using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;

namespace MicroserviceApiKernel;

public class ScopeRequirement : IAuthorizationRequirement
{
    public ScopeRequirement(string scope)
    {
        Scope = scope;
    }

    public string Scope { get; }
}

public class ScopeRequirementHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        var roleClaims = context.User.Claims.Where(c =>
    c.Type == "scope" ||
    c.Type == JwtClaimTypes.Scope);

        var hasMatchingRole = roleClaims.Any(c =>
            string.Equals(c.Value, requirement.Scope, StringComparison.OrdinalIgnoreCase));

        if (hasMatchingRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}