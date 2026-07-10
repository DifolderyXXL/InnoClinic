using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Accounts.GetAccounts;

public record GetAccountsQuery(int Page = 1, int PageSize = 50) : IQuery<GetAccountsResponse>;

public record GetAccountsResponse(List<AccountDto> Items, int Page, int PageSize, int Total);

public class GetAccountsQueryHandler(ProfilesDbContext context) : IQueryHandler<GetAccountsQuery, GetAccountsResponse>
{
    public async Task<Result<GetAccountsResponse>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        var total = await context.Accounts.CountAsync(ct);
        var items = await context.Accounts
            .OrderBy(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectToType<AccountDto>()
            .ToListAsync(ct);

        return new GetAccountsResponse(items, query.Page, query.PageSize, total);
    }
}
