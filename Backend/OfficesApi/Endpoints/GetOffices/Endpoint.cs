using System;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using OfficesApi.Infrastructure;

namespace OfficesApi.Endpoints.GetOffices;

public class GetOfficesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/offices", async (
            [AsParameters] PaginationParameters pagination,
            IQueryHandler<GetOfficesQuery, GetOfficesResponse> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new(pagination), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(x));
        }).AllowAnonymous();

        builder.MapGet("/offices/{id}", async (string id, OfficesDbContext context, CancellationToken ct) =>
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

            return result.MapToTypedResult(TypedResults.Ok);
        }).AllowAnonymous();
    }
}
