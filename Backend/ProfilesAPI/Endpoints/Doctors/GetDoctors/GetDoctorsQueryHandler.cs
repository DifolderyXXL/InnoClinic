using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public record GetDoctorsQuery() : IQuery<GetDoctorsResponse>;

public record GetDoctorsResponse(List<DoctorDto> Doctors);

public class GetDoctorsQueryHandler(ProfilesDbContext context) : IQueryHandler<GetDoctorsQuery, GetDoctorsResponse>
{
    public async Task<Result<GetDoctorsResponse>> Handle(GetDoctorsQuery query, CancellationToken ct)
    {
        var doctors = await context.Doctors
            .ProjectToType<DoctorDto>()
            .ToListAsync(ct);

        return new GetDoctorsResponse(
            doctors
        );
    }
}