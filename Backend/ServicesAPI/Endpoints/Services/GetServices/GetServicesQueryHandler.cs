using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;
using ServicesAPI.Models;

namespace ServicesAPI.Endpoints.Services.GetServices;

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
            .Map(dest => dest.SpecializationName, src => src.Specialization.SpecializationName)
            .Map(dest => dest.SlotLength, src => src.ServiceCategory.TimeSlotSize);
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
    int SlotLength,
    long CategoryId,
    string CategoryName,
    long SpecializationId,
    string SpecializationName);

public record GetServicesQuery(long? CategoryId = null, long? SpecializationId = null) : IQuery<GetServicesResponse>;

public record GetServicesResponse(List<ServiceDto> Services);