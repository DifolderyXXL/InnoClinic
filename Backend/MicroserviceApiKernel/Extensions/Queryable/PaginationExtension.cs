using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceApiKernel.Extensions.Queryable;


public record PaginationParameters(int Page = 1, int PageSize = 50);
public static class PaginationExtension
{
    extension<TSource>(IQueryable<TSource> source)
    {
        public IQueryable<TSource> Pagination(int page, int pageSize)
        {
            return source
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
        public IQueryable<TSource> Pagination(PaginationParameters parameters)
        {
            return source.Pagination(parameters.Page, parameters.PageSize);
        }
    }
}

public record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount);
    
public static class PaginationExtensions
{
    public static async Task<PagedResponse<TDestination>> ToPagedResponseAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        PaginationParameters pagination,
        Expression<Func<TSource, TDestination>> mapExpression,
        CancellationToken ct = default)
    {
        var totalCount = await query.LongCountAsync(ct);

        var items = await query
            .Pagination(pagination)
            .Select(mapExpression)
            .ToListAsync(ct);

        return new PagedResponse<TDestination>(items, pagination.Page, pagination.PageSize , totalCount);
    }
}