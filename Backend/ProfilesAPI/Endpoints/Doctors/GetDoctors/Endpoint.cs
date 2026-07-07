using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/doctors", async (IQueryHandler<GetDoctorsQuery, GetDoctorsResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(x));
        }).RequireAuthorization(RolePolicy.Client);
    }
}
