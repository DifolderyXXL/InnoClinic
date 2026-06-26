using System;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace OfficesApi.Endpoints.ChangeOfficeActive;


public class Endpoint : IEndpoint
{
    public record Request(bool IsActive);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPatch("/api/office/{id}", async (
            string id,
            [FromBody] Request request,
            ICommandHandler<ChangeOfficeActiveCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(id, request.IsActive), ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}
