using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.Services.GetServices;

public class GetServicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/services", async (
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
    }
}

public class GetServicesQueryHandler(ServicesDbContext context)
    : IQueryHandler<GetServicesQuery, GetServicesResponse>
{
    private static readonly TypeAdapterConfig LocalMapsterConfig = CreateMapsterConfig();

    private static TypeAdapterConfig CreateMapsterConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<Service, ServiceDto>()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.SpecializationId, src => src.SpecializationId)
            .Map(dest => dest.CategoryName, src => src.ServiceCategory.CategoryName)
            .Map(dest => dest.SpecializationName, src => src.Specialization.SpecializationName);
        return config;
    }

    public async Task<Result<GetServicesResponse>> Handle(GetServicesQuery query, CancellationToken ct)
    {
        var dbQuery = context.Services.AsNoTracking();

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.CategoryId == query.CategoryId.Value);
        }

        if (query.SpecializationId.HasValue)
        {
            dbQuery = dbQuery.Where(s => s.SpecializationId == query.SpecializationId.Value);
        }

        var services = await dbQuery
            .ProjectToType<ServiceDto>(LocalMapsterConfig)
            .ToListAsync(ct);

        return new GetServicesResponse(services);
    }
}

public record ServiceDto(
    long Id,
    string ServiceName,
    decimal Price,
    bool IsActive,
    long CategoryId,
    string CategoryName,
    long SpecializationId,
    string SpecializationName);

public record GetServicesQuery(long? CategoryId = null, long? SpecializationId = null) : IQuery<GetServicesResponse>;

public record GetServicesResponse(List<ServiceDto> Services);