using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;

namespace ProfilesAPI.Endpoints.User.GetProfiles;

public class GetUserProfilesQueryHandler(ProfilesDbContext context) : IQueryHandler<GetUserProfileQuery, GetUserProfileQueryResponse>
{
    public async Task<Result<GetUserProfileQueryResponse>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var accountQuery = context.Accounts.AsQueryable();

        if (query.Roles.Contains(ConstantRoles.Patient))
        {
            accountQuery = accountQuery.Include(x => x.Patient);
        }
        
        if (query.Roles.Contains(ConstantRoles.Doctor))
        {
            accountQuery = accountQuery
                .Include(x => x.Doctor)
                    .ThenInclude(d => d.Specialization);;
        }

        if (query.Roles.Contains(ConstantRoles.Receptionist))
        {
            accountQuery = accountQuery.Include(x => x.Receptionist);
        }
        
        var account = await accountQuery
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.Id == query.UserId, ct);

        if (account == null) return ProfileErrors.AccountNotFoundError();

        var baseAccount = account.ToDto();

        var response = new GetUserProfileQueryResponse(
            baseAccount,
            account.Patient?.ToDto(),
            account.Doctor?.ToDto(),
            account.Receptionist?.ToDto()
                );

        return response;
    }
}
public record GetUserProfileQueryResponse(BaseAccountDto Account, OnlyPatientDto? Patient, OnlyDoctorDto? Doctor, OnlyReceptionistDto? Receptionist);
public record GetUserProfileQuery(Guid UserId, string[] Roles) : IQuery<GetUserProfileQueryResponse>;

public static class ProfileErrors
{
    public static Error AccountNotFoundError() => Error.Create(ErrorType.NotFound);
}