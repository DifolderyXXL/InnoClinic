using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public class GetDoctorByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/doctors/{id:long}", async (
            long id,
            IQueryHandler<GetDoctorByIdQuery, GetDoctorByIdResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Client);
    }
}
