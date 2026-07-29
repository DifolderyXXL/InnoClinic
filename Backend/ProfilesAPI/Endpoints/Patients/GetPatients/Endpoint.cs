using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ProfilesAPI.Endpoints.Patients.GetPatients;

public class Endpoint : IPatientEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/patients", async (
            [AsParameters] GetPatientsQuery query,
            IQueryHandler<GetPatientsQuery, GetPatientsResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Patients.Read);
    }
}
