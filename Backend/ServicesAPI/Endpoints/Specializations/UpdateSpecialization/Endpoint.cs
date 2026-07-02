using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Specializations.UpdateSpecialization;

public class UpdateSpecializationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/api/specialization/{id:long}", async (
            long id,
            UpdateSpecializationCommand request,
            ICommandHandler<UpdateSpecializationCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Specialization);
    }
}
