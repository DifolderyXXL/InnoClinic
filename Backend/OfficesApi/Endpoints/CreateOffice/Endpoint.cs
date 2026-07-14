using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OfficesApi.Endpoints.CreateOffice;

public class Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/offices", async (
            [FromBody] CreateOfficeCommand request,
            ICommandHandler<CreateOfficeCommand, CreateOfficeResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(
                new(request.PhotoId, request.City, request.Street, request.HouseNumber, request.OfficeNumber, request.RegistryPhoneNumber, request.IsActive), ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}

