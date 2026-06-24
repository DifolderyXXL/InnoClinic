using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OfficesApi.Endpoints.CreateOffice;

public class Endpoint : IEndpoint
{
    public record Request(long? PhotoId, string City, string Street, string HouseNumber, string RegistryPhoneNumber, string IsActive);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/create", async (
            [FromBody] Request request,
            ICommandHandler<CreateOfficeCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(
                new(request.PhotoId, request.City, request.Street, request.HouseNumber, request.RegistryPhoneNumber, request.IsActive), ct);

            return result.MapToTypedResult(() => TypedResults.Created());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}

