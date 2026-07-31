using Mapster;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Application;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Doctors.GetDoctors;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public record GetDoctorByIdQuery(Guid Id) : IQuery<GetDoctorByIdResponse>;

public record GetDoctorByIdResponse(DoctorDto DoctorDto);

public class GetDoctorByIdQueryHandler(ProfilesDbContext context, IPhotoUrlFactory photoUrlFactory) : IQueryHandler<GetDoctorByIdQuery, GetDoctorByIdResponse>
{   
    public async Task<Result<GetDoctorByIdResponse>> Handle(GetDoctorByIdQuery query, CancellationToken ct)
    {
        var result = await context.Doctors
            .Where(x => x.AccountId == query.Id)
            .ProjectToType<DoctorDto>(DoctorHelper.Config)
            .FirstOrDefaultAsync(cancellationToken: ct);

        if (result == null)
            return DoctorErrors.DoctorNotFound();

        var doctor = result;
        if (doctor.AccountPhotoId.HasValue)
        {
            doctor.PhotoUrl = photoUrlFactory.GenerateDoctorPhotoUrl(doctor.AccountId, doctor.AccountPhotoId.Value);
        }

        return new GetDoctorByIdResponse(doctor);
    }
}
