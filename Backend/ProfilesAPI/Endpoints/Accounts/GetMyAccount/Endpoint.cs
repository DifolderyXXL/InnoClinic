using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
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
            IPhotoUrlFactory urlFactory,
            CancellationToken ct) =>
        {
            var result = await Handle(context, user, urlFactory, ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization();
    }

    public static async Task<Result<AccountDto>> Handle(ProfilesDbContext context,
        UserClaimInfo user,
        IPhotoUrlFactory urlFactory,
        CancellationToken ct)
    {
        var guid = Guid.Parse(user.Id);
            
        var account = await context.Accounts
            .Where(x=>x.Id == guid)
            .ProjectToType<AccountDto>()
            .FirstOrDefaultAsync(ct);

        if (account == null)
        {
            return AccountErrors.NotFound();
        }

        if (account.PhotoId.HasValue)
        {
            account.PhotoUrl = urlFactory.GenerateMeUserPhotoUrl(account.PhotoId.Value);
        }

        return Result.Success(account);
    }
}
