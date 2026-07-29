using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Specializations.UpdateSpecialization;

public class UpdateSpecializationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/specializations/{id:long}", async (
            long id,
            UpdateSpecializationCommand request,
            ICommandHandler<UpdateSpecializationCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Specializations.Manage).WithTags(EndpointTags.Specialization);
    }
}
