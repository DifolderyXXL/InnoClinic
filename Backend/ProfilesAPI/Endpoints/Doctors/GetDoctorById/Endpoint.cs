using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public class GetDoctorByIdEndpoint : IDoctorEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/doctors/{id:guid}", async (
            Guid id,
            IQueryHandler<GetDoctorByIdQuery, GetDoctorByIdResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Client);
    }
}
