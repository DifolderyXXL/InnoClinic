using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public record GetDoctorByIdQuery(long Id) : IQuery<GetDoctorByIdResponse>;

public record GetDoctorByIdResponse(DoctorDto DoctorDto);

public class GetDoctorByIdQueryHandler(ProfilesDbContext context) : IQueryHandler<GetDoctorByIdQuery, GetDoctorByIdResponse>
{
    public async Task<Result<GetDoctorByIdResponse>> Handle(GetDoctorByIdQuery query, CancellationToken ct)
    {
        var result = await context.Doctors
            .Where(x => x.Id == query.Id)
            .ProjectToType<DoctorDto>()
            .FirstOrDefaultAsync();

        if (result == null)
            return DoctorErrors.DoctorNotFound();

        return new GetDoctorByIdResponse(result);
    }
}
