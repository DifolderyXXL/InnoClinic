using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;

namespace OfficesApi.Endpoints.UpdateOffice;

public class Endpoint : IEndpoint
{
    public record Request(Guid? PhotoId, string? City, string? Street, string? HouseNumber, string? OfficeNumber, string? RegistryPhoneNumber, bool? IsActive);
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/offices/{id}", async (string id, Request request, ICommandHandler<UpdateOfficeCommand> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new UpdateOfficeCommand(
                OfficeId: id,
                PhotoId: request.PhotoId,
                City: request.City,
                Street: request.Street,
                HouseNumber: request.HouseNumber,
                OfficeNumber: request.OfficeNumber,
                RegistryPhoneNumber: request.RegistryPhoneNumber,
                IsActive: request.IsActive
            ), ct);

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).HasPermissions(Permissions.Offices.Manage);
    }
}
