using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.Specializations.GetSpecialization;

public class GetSpecializationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/specializations", async (
            [FromQuery] bool? onlyActive,
            IQueryHandler<GetSpecializationsQuery, GetSpecializationsResponse> handler,
            CancellationToken ct
        ) =>
        {
            var query = new GetSpecializationsQuery(onlyActive);
            var result = await handler.Handle(query, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).AllowAnonymous().WithTags(EndpointTags.Specialization);
    }
}
public record SpecializationDto(long Id, string SpecializationName, bool IsActive);

public record GetSpecializationsQuery(bool? OnlyActive = null) : IQuery<GetSpecializationsResponse>;

public record GetSpecializationsResponse(List<SpecializationDto> Specializations);
public class GetSpecializationsQueryHandler(ServicesDbContext context)
    : IQueryHandler<GetSpecializationsQuery, GetSpecializationsResponse>
{
    public async Task<Result<GetSpecializationsResponse>> Handle(GetSpecializationsQuery query, CancellationToken ct)
    {
        var dbQuery = context.Specializations.AsNoTracking();

        if (query.OnlyActive.HasValue && query.OnlyActive.Value)
        {
            dbQuery = dbQuery.Where(s => s.IsActive);
        }

        var specializations = await dbQuery
            .ProjectToType<SpecializationDto>()
            .ToListAsync(ct);

        return new GetSpecializationsResponse(specializations);
    }
}