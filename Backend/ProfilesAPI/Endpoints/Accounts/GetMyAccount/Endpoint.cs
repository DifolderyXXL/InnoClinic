using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Accounts.GetMyAccount;

public class GetAccountEndpoint : IAccountEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("accounts/me", async(
            ProfilesDbContext context,
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);
            
            var account = await context.Accounts
                .Where(x=>x.Id == guid)
                .ProjectToType<AccountDto>()
                .FirstOrDefaultAsync(ct);

            var result = account == null ? AccountErrors.NotFound() : Result.Success(account);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization();
    }
}
