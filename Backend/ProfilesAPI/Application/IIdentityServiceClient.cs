using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Application;

public record CreateIdentityUserResponse(string UserId, string SetPasswordLink);
public record GetUserByEmailResponse(Guid UserId);
public interface IIdentityServiceClient
{
    Task<Result<CreateIdentityUserResponse>> CreateIdentityUserAsync(
        string email, 
        List<string> roles, 
        CancellationToken ct);
    
    Task<Result<GetUserByEmailResponse>> GetIdentityUserAsync(
        string email, 
        CancellationToken ct);


    public Task<Result> DeleteIdentityUserAsync(Guid userId, CancellationToken ct);
}