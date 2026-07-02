using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace ServicesAPI.Endpoints.Specializations.CreateSpecialization;

public class CreateSpecializationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/create/specialization", async (
            CreateSpecializationCommand request,
            ICommandHandler<CreateSpecializationCommand> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(request, ct);

            return result.MapToTypedResult(TypedResults.Created);
        }).RequireAuthorization(RolePolicy.Receptionist).WithTags(EndpointTags.Specialization);
    }
}