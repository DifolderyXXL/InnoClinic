using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Services.UpdateService;

public class UpdateServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/service/{id:long}", async (
            long id,
            UpdateServiceCommand request,
            ICommandHandler<UpdateServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Services);
    }
}
