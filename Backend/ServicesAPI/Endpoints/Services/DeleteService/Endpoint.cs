using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Services.DeleteService;

public class DeleteServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/service/{id:long}", async (
            long id,
            ICommandHandler<DeleteServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteServiceCommand(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Services);
    }
}
