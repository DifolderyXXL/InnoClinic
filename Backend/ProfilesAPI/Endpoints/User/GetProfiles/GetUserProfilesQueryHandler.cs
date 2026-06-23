using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.User.GetProfiles;

namespace ProfilesAPI.Endpoints.User;

public class GetUserProfilesQueryHandler(ProfilesDbContext context) : IQueryHandler<GetUserProfileQuery, GetUserProfileQueryResponse>
{
    public async Task<Result<GetUserProfileQueryResponse>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var account = await context.Accounts
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.Id == query.UserId, ct);

        if (account == null) return ProfileErrors.AccountNotFoundError();

        var baseAccount = account.ToDto();

        var response = new GetUserProfileQueryResponse(
                account.Patient.ToDto(baseAccount),
                query.Roles.Any(x => x == ConstantRoles.Doctor) ? account.Doctor.ToDto(baseAccount) : null,
                query.Roles.Any(x => x == ConstantRoles.Receptionist) ? account.Receptionist.ToDto(baseAccount) : null
                );

        return response;
    }
}
public record GetUserProfileQueryResponse(PatientDto? Patient, DoctorDto? Doctor, ReceptionistDto? Receptionist);
public record GetUserProfileQuery(Guid UserId, string[] Roles) : IQuery<GetUserProfileQueryResponse>;

public static class ProfileErrors
{
    public static Error AccountNotFoundError() => Error.Create(ErrorType.NotFound);
}