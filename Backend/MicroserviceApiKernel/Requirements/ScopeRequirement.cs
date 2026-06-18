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
        if (context.User.Claims.Any(c => (c is { Type: "scope" or JwtClaimTypes.Scope }) && c.Value == requirement.Scope))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}