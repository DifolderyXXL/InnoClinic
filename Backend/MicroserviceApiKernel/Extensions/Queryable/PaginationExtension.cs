namespace MicroserviceApiKernel.Extensions.Queryable;

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
    }
}