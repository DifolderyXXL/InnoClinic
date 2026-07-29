using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Services.DeleteService;

public class DeleteServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/services/{id:long}", async (
            long id,
            ICommandHandler<DeleteServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteServiceCommand(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Services.Manage).WithTags(EndpointTags.Services);
    }
}
