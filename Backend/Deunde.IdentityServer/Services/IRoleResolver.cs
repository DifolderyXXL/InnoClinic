using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public interface IRoleResolver
{
    Task<Claim> ResolveUserRoleClaimAsync(IdentityUser user, IEnumerable<string>? acrValues);
}