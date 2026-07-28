using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ServicesAPI.Endpoints.Services.GetServices;

public class GetServicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/services", async (
            [FromQuery] long? categoryId,
            [FromQuery] long? specializationId,
            IQueryHandler<GetServicesQuery, GetServicesResponse> handler,
            CancellationToken ct
        ) =>
        {
            var query = new GetServicesQuery(categoryId, specializationId);
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).AllowAnonymous().WithTags(EndpointTags.Services);
        
        builder.MapGet("/services/{serviceId:long}", async (
            [FromRoute] long serviceId,
            IQueryHandler<GetServiceByIdQuery, ServiceDto> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(new(serviceId), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).AllowAnonymous().WithTags(EndpointTags.Services);
    }
}

