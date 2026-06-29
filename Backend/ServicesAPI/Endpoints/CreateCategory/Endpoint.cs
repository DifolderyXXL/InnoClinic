using System;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ServicesAPI.Endpoints.CreateCategory;

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/create/category", async (
            CreateCategoryCommand request,
            ICommandHandler<CreateCategoryCommand> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(() => TypedResults.Created());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}