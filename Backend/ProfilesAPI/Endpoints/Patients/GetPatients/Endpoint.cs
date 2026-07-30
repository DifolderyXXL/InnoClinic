using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Extensions.Queryable;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

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
        
        builder.MapGet("/patients/{id:guid}", async Task<Results<NotFound, Ok<PatientDto>>>(
            [FromRoute] Guid id,
            ProfilesDbContext context,
            CancellationToken ct) =>
        {
            var item = await context.Patients
                .Include(p => p.Account)
                .Where(x=>x.AccountId == id)
                .ProjectToType<PatientDto>()
                .FirstOrDefaultAsync(ct);

            if (item == null)
            {
                return TypedResults.NotFound();
            }
            
            return TypedResults.Ok(item);

        }).HasPermissions(Permissions.Patients.Read);
    }
}
