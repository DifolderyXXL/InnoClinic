using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Receptionists.GetReceptionists;

public record GetReceptionistsQuery(int Page = 1, int PageSize = 50) : IQuery<GetReceptionistsResponse>;

public record GetReceptionistsResponse(List<ReceptionistDto> Items, int Page, int PageSize, int Total);

public class GetReceptionistsQueryHandler(ProfilesDbContext context) : IQueryHandler<GetReceptionistsQuery, GetReceptionistsResponse>
{
    public async Task<Result<GetReceptionistsResponse>> Handle(GetReceptionistsQuery query, CancellationToken ct)
    {
        var total = await context.Receptionists.CountAsync(ct);
        var items = await context.Receptionists
            .Include(r => r.Account)
            .OrderBy(x => x.Id)
            .Pagination(query.Page, query.PageSize)
            .ProjectToType<ReceptionistDto>()
            .ToListAsync(ct);

        return new GetReceptionistsResponse(items, query.Page, query.PageSize, total);
    }
}
