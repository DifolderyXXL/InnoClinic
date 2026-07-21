using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public class Endpoint : IDoctorEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/doctors", async (
            [AsParameters] GetDoctorsQuery query,
            IQueryHandler<GetDoctorsQuery, GetDoctorsResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).Produces<GetDoctorsResponse>(StatusCodes.Status200OK)
            .RequireAuthorization(RolePolicy.Client);
    }
}
