using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Patients.GetPatients;

public record GetPatientsQuery(int Page = 1, int PageSize = 50) : IQuery<GetPatientsResponse>;

public record GetPatientsResponse(List<PatientDto> Items, int Page, int PageSize, int Total);

public class GetPatientsQueryHandler(ProfilesDbContext context) : IQueryHandler<GetPatientsQuery, GetPatientsResponse>
{
    public async Task<Result<GetPatientsResponse>> Handle(GetPatientsQuery query, CancellationToken ct)
    {
        var total = await context.Patients.CountAsync(ct);
        var items = await context.Patients
            .Include(p => p.Account)
            .OrderBy(x => x.Id)
            .Pagination(query.Page, query.PageSize)
            .ProjectToType<PatientDto>()
            .ToListAsync(ct);

        return new GetPatientsResponse(items, query.Page, query.PageSize, total);
    }
}
