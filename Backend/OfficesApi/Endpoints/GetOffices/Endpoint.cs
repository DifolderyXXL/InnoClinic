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
        builder.MapGet("/offices", async (IQueryHandler<GetOfficesQuery, GetOfficesResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(x));
        }).RequireAuthorization(RolePolicy.Client);

        builder.MapGet("/office/{id}", async (string id, OfficesDbContext context, CancellationToken ct) =>
        {
            var officeResult = await context.GetOffice(id, ct);

            var office = officeResult.Value;

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
                    OfficeNumber: office.OfficeNumber,
                    RegistryPhoneNumber: office.RegistryPhoneNumber,
                    IsActive: office.IsActive
                ));
            }

            return result.MapToTypedResult(x => TypedResults.Ok(x));
        }).RequireAuthorization(RolePolicy.Client);
    }
}
