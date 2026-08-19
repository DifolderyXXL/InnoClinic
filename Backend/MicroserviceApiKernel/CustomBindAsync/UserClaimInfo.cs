using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ProfilesAPI.CustomBindAsync;

public class UserClaimInfo
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string[] Roles { get; init; } = null!;
    public bool EmailVerified { get; init; } = false;
    public ClaimsPrincipal ClaimsPrincipal { get; init; } = null!;

    public static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirst("sub")?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    
    public static ValueTask<UserClaimInfo?> BindAsync(HttpContext context,
                                                   ParameterInfo parameter)
    {
        var user = context.User;

        var userId = user.FindFirst("sub")?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var email = user.FindFirst("email")?.Value
                 ?? user.FindFirst(ClaimTypes.Email)?.Value;

        var roles = user.FindAll("role")
                 ?? user.FindAll(ClaimTypes.Role);

        var emailVerified = user.FindFirstValue("emailVerified") switch
        {
            "true" => true,
            "false" => false,
            _ => false
        };

        if (userId == null || email == null)
        {
            return ValueTask.FromResult<UserClaimInfo?>(null);
        }

        var result = new UserClaimInfo
        {
            Id = userId,
            Email = email,
            Roles = [.. roles.Select(x => x.Value)],
            EmailVerified = emailVerified,
            ClaimsPrincipal = user
        };

        return ValueTask.FromResult<UserClaimInfo?>(result);
    }
}