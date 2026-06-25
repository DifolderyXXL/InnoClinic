using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using OfficesApi.Infrastructure;

namespace OfficesApi.Endpoints.ChangeOfficeActive;

public class Endpoint : IEndpoint
{
    public record OfficeStatusUpdate(bool IsActive);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/api/office/{id}", async (string id, OfficeStatusUpdate request, OfficesDbContext context, CancellationToken ct) =>
        {
            var result = await Query(id, request, context, ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }

    public async Task<Result> Query(string id, OfficeStatusUpdate request, OfficesDbContext context, CancellationToken ct)
    {
        var officeResult = await context.GetOffice(id, ct);

        if (officeResult.IsError)
        {
            return officeResult;
        }

        var office = officeResult.Value!;

        var result = await context.UpdateOfficeActive(office, request.IsActive, ct);

        return result;
    }
}
