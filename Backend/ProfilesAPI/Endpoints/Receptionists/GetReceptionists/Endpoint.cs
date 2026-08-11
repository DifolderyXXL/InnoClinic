using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Receptionists.GetReceptionists;

public class Endpoint : IReceptionistEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/receptionists/{id:guid}", async (
            [AsParameters] GetReceptionistsQuery query,
            IQueryHandler<GetReceptionistsQuery, GetReceptionistsResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Accounts.Read);
    }
}
