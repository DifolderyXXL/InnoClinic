using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public class GetDoctorByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/doctor/{id:long}", async (
            long id,
            IQueryHandler<GetDoctorByIdQuery, GetDoctorByIdResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(id), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(x));
        }).RequireAuthorization(RolePolicy.Client);
    }
}
