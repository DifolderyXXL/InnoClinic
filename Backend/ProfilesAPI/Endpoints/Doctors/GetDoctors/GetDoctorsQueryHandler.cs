using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public record GetDoctorsQuery(int Page = 1, int PageSize = 50) : IQuery<GetDoctorsResponse>;

public record GetDoctorsResponse(List<DoctorDto> Items, int Page, int PageSize, int Total);

public class GetDoctorsQueryHandler(ProfilesDbContext context, IPhotoUrlFactory photoUrlFactory) : IQueryHandler<GetDoctorsQuery, GetDoctorsResponse>
{
    public async Task<Result<GetDoctorsResponse>> Handle(GetDoctorsQuery query, CancellationToken ct)
    {
        var total = await context.Doctors.CountAsync(ct);
        var doctors = await context.Doctors
            .OrderBy(x => x.Id)
            .Pagination(query.Page, query.PageSize)
            .ProjectToType<DoctorDto>()
            .ToListAsync(ct);

        foreach (var doctor in doctors)
        {
            if (doctor.AccountPhotoId.HasValue)
            {
                doctor.PhotoUrl = photoUrlFactory.GenerateDoctorPhotoUrl(doctor.AccountId, doctor.AccountPhotoId.Value);
            }
        }
        
        return new GetDoctorsResponse(
            doctors,
            query.Page,
            query.PageSize,
            total
        );
    }
}