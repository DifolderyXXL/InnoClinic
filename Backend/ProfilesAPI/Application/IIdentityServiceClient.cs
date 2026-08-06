using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Application;

public record CreateIdentityUserResponse(Guid UserId, string SetPasswordLink);

public record UserRegisteredIntegrationEvent(
    Guid UserId, 
    string Email, 
    string ConfirmationToken
);

public interface IIdentityServiceClient
{
    Task<Result<CreateIdentityUserResponse>> CreateIdentityUserAsync(
        string email, 
        List<string> roles, 
        CancellationToken ct);
}
