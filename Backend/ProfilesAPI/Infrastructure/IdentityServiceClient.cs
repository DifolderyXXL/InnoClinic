using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using ProfilesAPI.Application;

namespace ProfilesAPI.Infrastructure;

public class IdentityServiceClient(HttpClient client, IFrontendUrlGenerator frontendUrlGenerator) : IIdentityServiceClient
{
    public async Task<Result<CreateIdentityUserResponse>> CreateIdentityUserAsync(string email, List<string> roles, CancellationToken ct)
    {
        var payload = new { email, roles, returnUrl = frontendUrlGenerator.GenerateFrontendIndexUrl() };
        var response = await client.PostAsJsonAsync("users", payload, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ReadErrorAsync(ct);
            return error;
        }

        var service = await response.Content.ReadFromJsonAsync<CreateIdentityUserResponse>(cancellationToken: ct);

        if (service is null)
        {
            return new Error("NullResponse", "Received null response from Identity API.",  ErrorType.Problem);
        }

        return Result.Success(service);
    }

    public async Task<Result<GetUserByEmailResponse>> GetIdentityUserAsync(string email, CancellationToken ct)
    {
        var response = await client.GetAsync($"users/by-email/{email}", ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ReadErrorAsync(ct);
            return error;
        }

        var service = await response.Content.ReadFromJsonAsync<GetUserByEmailResponse>(cancellationToken: ct);

        if (service is null)
        {
            return new Error("NullResponse", "Received null response from Identity API.",  ErrorType.Problem);
        }

        return Result.Success(service);
    }
}