using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.UpdateService;

public class UpdateServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/api/service/{id:long}", async (
            long id,
            UpdateServiceCommand request,
            ICommandHandler<UpdateServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}
