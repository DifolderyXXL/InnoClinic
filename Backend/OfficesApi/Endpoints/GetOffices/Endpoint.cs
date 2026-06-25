using System;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using OfficesApi.Infrastructure;

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

        builder.MapGet("/api/office", async ([FromQuery] string officeId, OfficesDbContext context, CancellationToken ct) =>
        {
            var office = await context.GetOffice(officeId, ct);

            Result<OfficeDto> result;
            if (office == null)
            {
                result = OfficeErrors.NotFound();
            }
            else
            {
                result = Result.Success(new OfficeDto(
                    Id: office.Id.ToString(),
                    PhotoId: office.PhotoId,
                    City: office.City,
                    Street: office.Street,
                    HouseNumber: office.HouseNumber,
                    RegistryPhoneNumber: office.RegistryPhoneNumber,
                    IsActive: office.IsActive
                ));
            }

            return result.MapToTypedResult(x => TypedResults.Ok(result));
        }).RequireAuthorization(RolePolicy.Client);
    }
}

public static class OfficeErrors
{
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}
