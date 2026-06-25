using System;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace OfficesApi.Endpoints.GetOffices;

public class GetOfficesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/offices", async (IQueryHandler<GetOfficesQuery, GetOfficesResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(result));
        }).RequireAuthorization(RolePolicy.Client);
    }
}
