using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Services.CreateService;

public class CreateServiceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/services", async (
            CreateServiceCommand request,
            ICommandHandler<CreateServiceCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).HasPermissions(Permissions.Services.Manage).WithTags(EndpointTags.Services);
    }
}
