using Mapster;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.Categories.GetCategories;

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/categories", async (
            IQueryHandler<GetCategoriesQuery, GetCategoriesResponse> handler,
            CancellationToken ct
        ) =>
        {
            var result = await handler.Handle(new GetCategoriesQuery(), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).AllowAnonymous().WithTags(EndpointTags.Categories);
    }
}

public record CategoryDto(long Id, string CategoryName, uint TimeSlotSize);

public record GetCategoriesQuery() : IQuery<GetCategoriesResponse>;

public record GetCategoriesResponse(List<CategoryDto> Categories);
public class GetCategoriesQueryHandler(ServicesDbContext context)
    : IQueryHandler<GetCategoriesQuery, GetCategoriesResponse>
{
    public async Task<Result<GetCategoriesResponse>> Handle(GetCategoriesQuery query, CancellationToken ct)
    {
        var categories = await context.ServiceCategories
            .ProjectToType<CategoryDto>()
            .ToListAsync(ct);

        return new GetCategoriesResponse(categories);
    }
}