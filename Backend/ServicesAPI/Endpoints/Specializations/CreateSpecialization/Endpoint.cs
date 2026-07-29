using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Specializations.CreateSpecialization;

public class CreateSpecializationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/specializations", async (
            CreateSpecializationCommand request,
            ICommandHandler<CreateSpecializationCommand> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).HasPermissions(Permissions.Specializations.Manage).WithTags(EndpointTags.Specialization);
    }
}