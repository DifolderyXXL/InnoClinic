using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Categories.CreateCategory;

public class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/categories", async (
            CreateCategoryCommand request,
            ICommandHandler<CreateCategoryCommand> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Categories);
    }
}