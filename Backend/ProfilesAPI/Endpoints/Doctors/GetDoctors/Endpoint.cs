using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Extensions.Queryable;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public class Endpoint : IDoctorEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/doctors", async (
            [AsParameters] PaginationParameters pagination,
            [AsParameters] GetDoctorsFilters getDoctorsFilters,
            IQueryHandler<GetDoctorsQuery, GetDoctorsResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(pagination, getDoctorsFilters), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).Produces<GetDoctorsResponse>(StatusCodes.Status200OK)
            .HasPermissions(Permissions.Doctors.Read);
    }
}
