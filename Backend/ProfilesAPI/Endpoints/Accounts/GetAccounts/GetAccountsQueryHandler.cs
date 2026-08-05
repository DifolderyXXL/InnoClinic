using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Accounts.Create;

namespace ProfilesAPI.Endpoints.Accounts.GetAccounts;

public record GetAccountsQuery(int Page = 1, int PageSize = 50) : IQuery<GetAccountsResponse>;

public record GetAccountsResponse(List<AccountDto> Items, int Page, int PageSize, int Total);

public class GetAccountsQueryHandler(ProfilesDbContext context, IPhotoUrlFactory photoUrlFactory) : IQueryHandler<GetAccountsQuery, GetAccountsResponse>
{
    public async Task<Result<GetAccountsResponse>> Handle(GetAccountsQuery query, CancellationToken ct)
    {
        var total = await context.Accounts.CountAsync(ct);
        var items = await context.Accounts
            .OrderBy(x => x.Id)
            .Pagination(query.Page, query.PageSize)
            .ProjectToType<AccountDto>()
            .ToListAsync(ct);
        
        foreach (var account in items)
        {
            if (account.PhotoId.HasValue)
            {
                account.PhotoUrl = photoUrlFactory.GenerateUserPhotoUrl(account.Id, account.PhotoId.Value);
            }
        }

        return new GetAccountsResponse(items, query.Page, query.PageSize, total);
    }
}

public record GetAccountById(Guid Id) : IQuery<AccountDto>;

public class GetAccountByIdQueryHandler(ProfilesDbContext context, IPhotoUrlFactory photoUrlFactory) : IQueryHandler<GetAccountById, AccountDto>
{
    public async Task<Result<AccountDto>> Handle(GetAccountById query, CancellationToken ct)
    {
        var account = await context.Accounts
            .Where(x => x.Id == query.Id)
            .ProjectToType<AccountDto>()
            .FirstOrDefaultAsync(ct);

        if (account is null)
        {
            return AccountErrors.NotFound();
        }

        if (account.PhotoId.HasValue)
        {
            account.PhotoUrl = photoUrlFactory.GenerateUserPhotoUrl(account.Id, account.PhotoId.Value);
        }

        return account;
    }
}