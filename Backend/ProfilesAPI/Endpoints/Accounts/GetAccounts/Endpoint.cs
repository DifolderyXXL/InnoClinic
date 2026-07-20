using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Accounts.GetAccounts;

public class Endpoint : IAccountEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/accounts", async (
            [AsParameters] GetAccountsQuery query,
            IQueryHandler<GetAccountsQuery, GetAccountsResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Client);
    }
}
