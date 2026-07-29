using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Categories.DeleteCategory;

public class DeleteCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/categories/{id:long}", async (
            long id,
            ICommandHandler<DeleteCategoryCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteCategoryCommand(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Services.Manage).WithTags(EndpointTags.Categories);
    }
}
