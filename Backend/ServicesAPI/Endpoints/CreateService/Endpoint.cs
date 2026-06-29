using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.CreateService;

public class CreateServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/create/service", async (
            CreateServiceCommand request,
            ICommandHandler<CreateServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(() => TypedResults.Created());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}
