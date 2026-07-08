using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Specializations.DeleteSpecialization;

public class DeleteSpecializationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/specializations/{id:long}", async (
            long id,
            ICommandHandler<DeleteSpecializationCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new DeleteSpecializationCommand(id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Specialization);
    }
}
