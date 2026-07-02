using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MicroserviceApiKernel;

public static class UserClaimParser
{
    public static ValueTask<UserClaimParserResult?> Parse(HttpContext context)
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
            return ValueTask.FromResult<UserClaimParserResult?>(null);
        }

        var result = new UserClaimParserResult
        {
            Id = userId,
            Email = email,
            Roles = [.. roles.Select(x => x.Value)],
            EmailVerified = emailVerified,
            ClaimsPrincipal = user
        };

        return ValueTask.FromResult<UserClaimParserResult?>(result);
    }
}


public class UserClaimParserResult
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string[] Roles { get; init; } = null!;
    public bool EmailVerified { get; init; } = false;
    public ClaimsPrincipal ClaimsPrincipal { get; init; } = null!;
}