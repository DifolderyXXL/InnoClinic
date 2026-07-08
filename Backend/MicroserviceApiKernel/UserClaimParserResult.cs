using System.Security.Claims;

namespace MicroserviceApiKernel;

public class UserClaimParserResult
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string[] Roles { get; init; } = null!;
    public bool EmailVerified { get; init; } = false;
    public ClaimsPrincipal ClaimsPrincipal { get; init; } = null!;
}